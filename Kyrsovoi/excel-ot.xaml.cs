﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using Excel = Microsoft.Office.Interop.Excel;

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для excel_ot.xaml
    /// </summary>
    public partial class excel_ot : Window
    {
        public excel_ot()
        {
            InitializeComponent();
        }

        public class ReportItem
        {
            public string Unit { get; set; }
            public int RentalCount { get; set; }
            public decimal TotalRevenue { get; set; }
        }

        private string connectionString = Class1.connection;

        private void CheckInDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckFields();
            if (CheckInDate.SelectedDate.HasValue)
            {
                if (Class1.add != 0)
                {
                    DateTime selectedDate = CheckInDate.SelectedDate.Value;
                    if (DateTime.Now < selectedDate)
                    {
                        CheckOutDate.DisplayDateStart = selectedDate.AddDays(1);
                        HighlightInvalidDates(CheckOutDate, DateTime.MinValue, selectedDate);
                        if (CheckOutDate.SelectedDate.HasValue && CheckOutDate.SelectedDate <= selectedDate)
                        {
                            CheckOutDate.SelectedDate = null;
                        }
                    }
                    else
                    {
                        CheckOutDate.Text = null;
                    }
                }
            }
            else
            {
                CheckOutDate.DisplayDateStart = null;
                CheckOutDate.BlackoutDates.Clear();
            }
        }

        private void HighlightInvalidDates(DatePicker datePicker, DateTime startDate, DateTime endDate)
        {
            datePicker.BlackoutDates.Clear();
            if (startDate < endDate)
            {
                datePicker.BlackoutDates.Add(new CalendarDateRange(startDate, endDate));
            }
        }

        private void CheckOutDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckFields();
            if (CheckOutDate.SelectedDate.HasValue)
            {
                if (Class1.add != 0)
                {
                    DateTime selectedDate = CheckOutDate.SelectedDate.Value;
                    if (DateTime.Now < selectedDate)
                    {
                        CheckInDate.DisplayDateEnd = selectedDate.AddDays(-1);
                        HighlightInvalidDates(CheckInDate, selectedDate, DateTime.MaxValue);
                        if (CheckInDate.SelectedDate.HasValue && CheckInDate.SelectedDate >= selectedDate)
                        {
                            CheckInDate.SelectedDate = null;
                        }
                    }
                    else
                    {
                        CheckOutDate.Text = null;
                    }
                }
            }
            else
            {
                CheckInDate.DisplayDateEnd = null;
                CheckInDate.BlackoutDates.Clear();
            }
        }

        private void CheckFields()
        {
            button.IsEnabled = CheckInDate.SelectedDate.HasValue && CheckOutDate.SelectedDate.HasValue;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckInDate.SelectedDate.HasValue || !CheckOutDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите даты начала и окончания периода.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime startDate = CheckInDate.SelectedDate.Value;
            DateTime endDate = CheckOutDate.SelectedDate.Value;

            try
            {
                List<ReportItem> reportData = GetReportData(startDate, endDate);
                if (reportData.Count == 0)
                {
                    MessageBox.Show("За указанный период нет данных для формирования отчета.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                GenerateOfficialWordReport(reportData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<ReportItem> GetReportData(DateTime startDate, DateTime endDate)
        {
            List<ReportItem> reportData = new List<ReportItem>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            gu.unit_name AS Unit,
                            COUNT(b.booking_id) AS RentalCount,
                            SUM(b.total_price) AS TotalRevenue
                        FROM 
                            glampingunits gu
                        LEFT JOIN 
                            bookings b ON b.unit_id = gu.unit_id
                            AND b.check_in_date >= @startDate
                            AND b.check_out_date <= @endDate
                            AND b.booking_status IN (1, 2)
                        GROUP BY 
                            gu.unit_name
                        HAVING 
                            COUNT(b.booking_id) > 0
                        ORDER BY 
                            TotalRevenue DESC;";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@startDate", startDate);
                        command.Parameters.AddWithValue("@endDate", endDate);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                reportData.Add(new ReportItem
                                {
                                    Unit = reader.GetString("Unit"),
                                    RentalCount = reader.GetInt32("RentalCount"),
                                    TotalRevenue = reader.GetDecimal("TotalRevenue")
                                });
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw; // Re-throw to be caught by the outer try-catch
                }
            }

            return reportData;
        }

        private void GenerateOfficialWordReport(List<ReportItem> report)
        {
            Excel.Application excelApp = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;

            try
            {
                excelApp = new Excel.Application();
                workbook = excelApp.Workbooks.Add();
                worksheet = workbook.ActiveSheet;

                int currentRow = 1;

                // Вставляем логотип (если есть)
                string logoPath = System.IO.Path.GetFullPath("Logo.png");
                if (File.Exists(logoPath))
                {
                    Excel.Picture picture = worksheet.Pictures().Insert(logoPath) as Excel.Picture;
                    picture.Top = worksheet.Cells[currentRow, 1].Top;
                    picture.Left = worksheet.Cells[currentRow, 1].Left;
                    picture.Width = 100;
                    picture.Height = 50;
                    currentRow += 3;
                }

                // Заголовок
                Excel.Range titleRange = worksheet.Range["A" + currentRow, "C" + currentRow];
                titleRange.Merge();
                titleRange.Value = "Отчёт по выручке по домам";
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 16;
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                currentRow += 2;

                // Подзаголовок
                Excel.Range subtitleRange = worksheet.Range["A" + currentRow, "C" + currentRow];
                subtitleRange.Merge();
                subtitleRange.Value = $"Дата формирования отчёта: {DateTime.Now:dd.MM.yyyy}";
                subtitleRange.Font.Italic = true;
                subtitleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                currentRow += 2;

                // Заголовки таблицы
                worksheet.Cells[currentRow, 1] = "Дом";
                worksheet.Cells[currentRow, 2] = "Количество аренд";
                worksheet.Cells[currentRow, 3] = "Общая выручка";

                Excel.Range headerRange = worksheet.Range["A" + currentRow, "C" + currentRow];
                headerRange.Font.Bold = true;
                headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                currentRow++;

                // Данные
                foreach (var item in report)
                {
                    worksheet.Cells[currentRow, 1] = item.Unit;
                    worksheet.Cells[currentRow, 2] = item.RentalCount;
                    worksheet.Cells[currentRow, 3] = item.TotalRevenue;
                    currentRow++;
                }

                // Итоговая строка
                decimal totalRevenue = report.Sum(item => item.TotalRevenue);
                worksheet.Cells[currentRow, 1] = "ИТОГО";
                Excel.Range totalRange = worksheet.Range["B" + currentRow, "C" + currentRow];
                totalRange.Merge();
                totalRange.Value = totalRevenue;
                totalRange.Font.Bold = true;
                totalRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Автоширина
                worksheet.Columns["A:C"].AutoFit();

                // Создаём путь
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string excelDirectory = System.IO.Path.Combine(baseDirectory, "Excel");
                Directory.CreateDirectory(excelDirectory); // Ensure directory exists
                string filePath = System.IO.Path.Combine(excelDirectory, $"RevenueReport_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx");

                // Сохраняем
                workbook.SaveAs(filePath);

                // Открываем файл Excel после создания
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });

                MessageBox.Show($"Отчёт успешно сохранён: {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Cleanup COM objects
                if (worksheet != null) Marshal.ReleaseComObject(worksheet);
                if (workbook != null)
                {
                    workbook.Close(false);
                    Marshal.ReleaseComObject(workbook);
                }
                if (excelApp != null)
                {
                    excelApp.Quit();
                    Marshal.ReleaseComObject(excelApp);
                }

                // Force garbage collection to clean up
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void Min_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void krest_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Prosmotr prosmotr = new Prosmotr();
            this.Hide();
            prosmotr.ShowDialog();
            this.Close();
        }

        private void ToolBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Window.GetWindow(this)?.DragMove();
            }
        }

        private void CheckInDate_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void CheckOutDate_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }
    }
}