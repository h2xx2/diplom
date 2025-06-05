-- MySQL dump 10.13  Distrib 9.1.0, for Win64 (x86_64)
--
-- Host: localhost    Database: glamping
-- ------------------------------------------------------
-- Server version	9.1.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `glamping`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `glamping` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `glamping`;

--
-- Table structure for table `booking_status`
--

DROP TABLE IF EXISTS `booking_status`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `booking_status` (
  `idbooking_status` int NOT NULL AUTO_INCREMENT,
  `booking_status` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`idbooking_status`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `booking_status`
--

LOCK TABLES `booking_status` WRITE;
/*!40000 ALTER TABLE `booking_status` DISABLE KEYS */;
INSERT INTO `booking_status` VALUES (1,'забронированный'),(2,'завершенный'),(3,'отмененный');
/*!40000 ALTER TABLE `booking_status` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `bookings`
--

DROP TABLE IF EXISTS `bookings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `bookings` (
  `booking_id` int NOT NULL AUTO_INCREMENT,
  `guest_id` int DEFAULT NULL,
  `unit_id` int DEFAULT NULL,
  `employees_id` int DEFAULT NULL,
  `check_in_date` date NOT NULL,
  `check_out_date` date NOT NULL,
  `total_price` decimal(10,2) DEFAULT NULL,
  `booking_status` int DEFAULT NULL,
  `upfront_payment` decimal(10,2) DEFAULT NULL,
  `pay_status` int DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`booking_id`),
  KEY `guest_id` (`guest_id`),
  KEY `unit_id` (`unit_id`),
  KEY `status_idx` (`booking_status`),
  KEY `status_pay_idx` (`pay_status`),
  KEY `empl_idx` (`employees_id`),
  CONSTRAINT `bookings_ibfk_1` FOREIGN KEY (`guest_id`) REFERENCES `guests` (`guest_id`),
  CONSTRAINT `bookings_ibfk_2` FOREIGN KEY (`unit_id`) REFERENCES `glampingunits` (`unit_id`),
  CONSTRAINT `status` FOREIGN KEY (`booking_status`) REFERENCES `booking_status` (`idbooking_status`),
  CONSTRAINT `status_pay` FOREIGN KEY (`pay_status`) REFERENCES `pay_status` (`idpay_status`)
) ENGINE=InnoDB AUTO_INCREMENT=365 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `bookings`
--

LOCK TABLES `bookings` WRITE;
/*!40000 ALTER TABLE `bookings` DISABLE KEYS */;
INSERT INTO `bookings` VALUES (1,1,1,1,'2024-01-11','2024-05-10',600.00,1,NULL,2,'2024-10-18 06:25:50'),(2,2,2,2,'2024-09-10','2024-09-12',300.00,3,NULL,2,'2024-10-18 06:25:50'),(3,3,3,3,'2024-10-15','2024-10-18',600.00,1,NULL,2,'2024-10-18 06:25:50'),(4,4,4,4,'2024-10-20','2024-10-22',420.00,2,NULL,1,'2024-10-18 06:25:50'),(5,5,5,5,'2024-11-01','2024-11-04',525.00,1,NULL,2,'2024-10-18 06:25:50'),(6,6,6,6,'2024-10-25','2024-10-28',540.00,3,NULL,2,'2024-10-18 06:25:50'),(7,7,7,7,'2024-10-10','2024-10-13',900.00,3,NULL,2,'2024-10-18 06:25:50'),(8,8,8,8,'2024-11-05','2024-11-08',960.00,1,NULL,2,'2024-10-18 06:25:50'),(9,9,9,9,'2024-11-10','2024-11-13',480.00,2,NULL,1,'2024-10-18 06:25:50'),(10,10,10,1,'2024-11-15','2024-11-19',850.00,3,NULL,2,'2024-10-18 06:25:50'),(11,11,11,2,'2024-11-20','2024-11-22',480.00,3,NULL,2,'2024-10-18 06:25:50'),(12,12,12,3,'2024-11-23','2024-11-26',560.00,1,NULL,2,'2024-10-18 06:25:50'),(13,13,13,4,'2024-11-27','2024-12-01',800.00,2,NULL,1,'2024-10-18 06:25:50'),(14,14,14,5,'2024-12-05','2024-12-08',675.00,3,NULL,2,'2024-10-18 06:25:50'),(15,15,15,6,'2024-12-10','2024-12-12',390.00,3,NULL,2,'2024-10-18 06:25:50'),(16,16,16,7,'2024-12-15','2024-12-18',630.00,1,NULL,2,'2024-10-18 06:25:50'),(17,17,17,8,'2024-12-20','2024-12-23',570.00,2,NULL,1,'2024-10-18 06:25:50'),(18,18,18,9,'2025-01-01','2025-01-05',900.00,3,NULL,2,'2024-10-18 06:25:50'),(19,19,19,12,'2025-01-06','2025-01-09',480.00,3,NULL,2,'2024-10-18 06:25:50'),(20,20,20,23,'2025-01-10','2025-01-13',640.00,1,NULL,2,'2024-10-18 06:25:50'),(21,21,21,21,'2025-01-15','2025-01-17',340.00,2,NULL,1,'2024-10-18 06:25:50'),(22,22,22,34,'2025-01-18','2025-01-21',680.00,3,NULL,2,'2024-10-18 06:25:50'),(23,23,23,5,'2025-01-25','2025-01-28',870.00,3,NULL,2,'2024-10-18 06:25:50'),(24,24,24,4,'2025-02-01','2025-02-05',800.00,1,NULL,2,'2024-10-18 06:25:50'),(25,25,25,3,'2025-02-10','2025-02-13',960.00,2,NULL,1,'2024-10-18 06:25:50'),(26,26,26,41,'2025-02-15','2025-02-19',950.00,3,NULL,2,'2024-10-18 06:25:50'),(27,27,27,3,'2025-02-20','2025-02-23',610.00,3,NULL,2,'2024-10-18 06:25:50'),(28,28,28,4,'2025-02-25','2025-02-27',420.00,1,NULL,2,'2024-10-18 06:25:50'),(29,29,29,8,'2025-03-01','2025-03-04',720.00,2,NULL,1,'2024-10-18 06:25:50'),(30,30,30,7,'2025-03-10','2025-03-13',810.00,3,NULL,2,'2024-10-18 06:25:50'),(31,31,31,6,'2025-03-15','2025-03-17',480.00,3,NULL,2,'2024-10-18 06:25:50'),(32,32,32,5,'2025-03-20','2025-03-22',560.00,1,NULL,2,'2024-10-18 06:25:50'),(33,33,33,6,'2025-03-25','2025-03-28',820.00,2,NULL,1,'2024-10-18 06:25:50'),(34,34,34,7,'2025-03-30','2025-04-03',1020.00,3,NULL,2,'2024-10-18 06:25:50'),(35,35,35,8,'2025-04-05','2025-04-08',690.00,3,NULL,1,'2024-10-18 06:25:50'),(36,36,36,9,'2025-04-10','2025-04-13',750.00,1,NULL,1,'2024-10-18 06:25:50'),(37,37,37,6,'2025-04-15','2025-04-18',780.00,2,NULL,2,'2024-10-18 06:25:50'),(38,8,38,32,'2025-04-20','2025-04-23',860.00,3,NULL,1,'2024-10-18 06:25:50'),(39,3,39,27,'2025-04-25','2025-04-28',520.00,3,NULL,2,'2024-10-18 06:25:50'),(40,4,40,29,'2025-04-30','2025-05-03',650.00,1,NULL,1,'2024-10-18 06:25:50'),(41,1,41,3,'2025-05-05','2025-05-08',690.00,2,NULL,2,'2024-10-18 06:25:50'),(42,2,42,4,'2025-05-10','2025-05-12',410.00,3,NULL,1,'2024-10-18 06:25:50'),(43,3,43,5,'2025-05-15','2025-05-18',830.00,3,NULL,2,'2024-10-18 06:25:50'),(44,4,44,3,'2025-05-20','2025-05-23',900.00,1,NULL,1,'2024-10-18 06:25:50'),(45,5,45,3,'2025-05-25','2025-05-28',670.00,2,NULL,1,'2024-10-18 06:25:50'),(46,6,46,3,'2025-06-01','2025-06-04',720.00,3,NULL,2,'2024-10-18 06:25:50'),(47,7,47,3,'2025-06-05','2025-06-07',430.00,3,NULL,1,'2024-10-18 06:25:50'),(48,8,48,5,'2025-06-10','2025-06-13',810.00,1,NULL,1,'2024-10-18 06:25:50'),(49,9,49,2,'2025-06-15','2025-06-18',950.00,2,NULL,1,'2024-10-18 06:25:50'),(50,5,5,3,'2025-06-20','2025-06-24',1000.00,3,NULL,2,'2024-10-18 06:25:50'),(355,1,1,2,'2025-06-01','2025-06-04',450.00,2,450.00,1,'2025-06-03 15:05:38'),(356,1,1,2,'2025-06-04','2025-06-19',2025.00,2,2025.00,1,'2025-06-03 15:07:40'),(357,1,1,2,'2025-06-04','2025-06-27',3105.00,1,3105.00,1,'2025-06-03 15:07:40'),(358,1,1,2,'2025-06-04','2025-06-27',3105.00,1,3105.00,1,'2025-06-03 15:10:03'),(359,2,1,2,'2025-06-04','2025-06-13',1350.00,2,1350.00,2,'2025-06-03 15:25:26'),(360,2,1,2,'2025-06-04','2025-06-20',2160.00,2,2160.00,1,'2025-06-03 15:31:58'),(361,2,2,2,'2025-05-30','2025-06-27',4032.00,2,4032.00,1,'2025-06-03 15:35:08'),(362,19,2,2,'2025-06-03','2025-06-26',3312.00,2,3312.00,1,'2025-06-03 15:39:54'),(363,1,2,2,'2025-05-29','2025-06-12',2016.00,1,604.80,2,'2025-06-04 09:26:03'),(364,1,1,2,'2025-06-05','2025-06-20',2025.00,1,607.50,2,'2025-06-04 09:45:03');
/*!40000 ALTER TABLE `bookings` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `employees`
--

DROP TABLE IF EXISTS `employees`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employees` (
  `employee_id` int NOT NULL AUTO_INCREMENT,
  `first_name` varchar(50) NOT NULL,
  `last_name` varchar(50) NOT NULL,
  `position` varchar(100) NOT NULL,
  `hire_date` date DEFAULT NULL,
  `phone` varchar(20) DEFAULT NULL,
  `email` varchar(100) NOT NULL,
  `login` varchar(45) DEFAULT NULL,
  `password` varchar(70) DEFAULT NULL,
  `role` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`employee_id`),
  UNIQUE KEY `email` (`email`)
) ENGINE=InnoDB AUTO_INCREMENT=44 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employees`
--

LOCK TABLES `employees` WRITE;
/*!40000 ALTER TABLE `employees` DISABLE KEYS */;
INSERT INTO `employees` VALUES (2,'Сергей','Петров','Координатор мероприятий','2020-03-10','555-0101','sergey.petrov@example.com','manager','6ee4a469cd4e91053847f5d3fcb61dbcc91e8f0ef10be7748da4c4a1ba382d17','Менеджер'),(3,'Елена','Сидорова','Специалист по продажам','2022-01-20','555-0102','elena.sidorova@example.com','admin','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918','Администратор'),(4,'Игорь','Кузнецов','Оператор глэмпинг-лагеря','2019-07-30','555-0103','igor.kuznetsov@example.com','','',''),(5,'Татьяна','Морозова','Дизайнер интерьеров','2021-11-15','555-0104','tatiana.morozova@example.com','','',''),(6,'Дмитрий','Федоров','Гид по экотуризму','2020-08-05','555-0105','dmitry.fedorov@example.com','','',''),(7,'Александра','Лебедева','Маркетолог','2018-09-10','555-0106','alexandra.lebedeva@example.com','','',''),(8,'Николай','Борисов','Специалист по безопасности','2023-02-12','555-0107','nikolai.borisov@example.com','','',''),(9,'Мария','Васильева','Кулинар','2021-04-18','555-0108','maria.vasilieva@example.com','','',''),(10,'Станислав','Ковалев','Логист','2020-06-30','555-0109','stanislav.kovalev@example.com','','',''),(11,'Ксения','Громова','Оператор по обслуживанию клиентов','2022-03-15','555-0110','kseniya.gromova@example.com','','',''),(12,'Андрей','Сергеев','Специалист по уборке','2019-12-01','555-0111','andrey.sergeev@example.com','','',''),(13,'Ольга','Давыдова','Руководитель проекта','2023-05-10','555-0112','olga.davydova@example.com','','',''),(14,'Виктор','Соловьев','Аниматор','2021-08-20','555-0113','victor.solovyev@example.com','','',''),(15,'Дарья','Захарова','Специалист по экологии','2020-02-14','555-0114','darya.zakharova@example.com','','',''),(16,'Максим','Павлов','Служитель лагеря','2018-10-25','555-0115','maksim.pavlov@example.com','','',''),(17,'Светлана','Степанова','Координатор по питанию','2021-03-22','555-0116','svetlana.stepanova@example.com','','',''),(18,'Григорий','Мельников','Экскурсовод','2019-09-12','555-0117','grigory.melniko@example.com','','',''),(19,'Анастасия','Фролова','Специалист по маркетингу','2022-06-10','555-0118','anastasia.frolova@example.com','','',''),(20,'Евгений','Семёнов','Кладовщик','2021-12-18','555-0119','evgeny.semenov@example.com','','',''),(21,'Людмила','Полякова','Кассир','2020-07-01','555-0120','lyudmila.polyakova@example.com','','',''),(22,'Роман','Савельев','Техник','2023-01-29','555-0121','roman.savelyev@example.com','','',''),(23,'Тимур','Никитин','Садовод','2022-05-14','555-0122','timur.nikitin@example.com','','',''),(24,'Алина','Крылова','Специалист по бронированию','2020-11-01','555-0123','alina.krylova@example.com','','',''),(25,'Федор','Тихонов','Специалист по логистике','2021-09-15','555-0124','fedor.tikhonov@example.com','','',''),(26,'Ирина','Семенова','Ведущий менеджер','2018-08-11','555-0125','irina.semenova@example.com','','',''),(27,'Галина','Кузьмина','Руководитель по работе с клиентами','2023-03-03','555-0126','galina.kuzmina@example.com','','',''),(28,'Валерий','Сидоров','Специалист по обслуживанию','2019-04-10','555-0127','valery.sidorov@example.com','','',''),(29,'Алексей','Егоров','Туристический агент','2022-10-05','555-0128','alexey.egorov@example.com','','',''),(30,'Маргарита','Сафонова','Специалист по услугам','2021-01-29','555-0129','margarita.safonova@example.com','','',''),(31,'Станислав','Беспалов','Менеджер по безопасности','2020-06-17','555-0130','stanislav.bespalov@example.com','','',''),(32,'Яна','Миронова','Руководитель группы','2022-07-11','555-0131','yana.mironova@example.com','','',''),(33,'Кирилл','Федосеев','Водитель','2020-12-20','555-0132','kirill.fedoseev@example.com','','',''),(34,'Нина','Шестакова','Помощник менеджера','2019-03-01','555-0133','nina.shestakova@example.com','','',''),(35,'Семен','Заболотный','Эколог','2021-06-12','555-0134','semen.zabolotny@example.com','','',''),(36,'Лариса','Левина','Репетитор по языкам','2023-04-02','555-0135','larisa.levina@example.com','','',''),(37,'Роман','Волков','Специалист по общественным связям','2022-09-21','555-0136','roman.volkov@example.com','','',''),(38,'Татьяна','Гришина','Диспетчер','2020-10-30','555-0137','tatiana.grishina@example.com','','',''),(39,'Арсений','Куликов','Слесарь','2021-11-19','555-0138','arseniy.kulikov@example.com','','',''),(40,'Наталья','Филимонова','Специалист по обслуживанию лагеря','2022-08-15','555-0139','natalya.filimonova@example.com','','',''),(41,'Денис','Сенкевич','Координатор по досугу','2020-05-20','555-0140','denis.senkevich@example.com','','','');
/*!40000 ALTER TABLE `employees` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `glampingunits`
--

DROP TABLE IF EXISTS `glampingunits`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `glampingunits` (
  `unit_id` int NOT NULL AUTO_INCREMENT,
  `unit_name` varchar(100) DEFAULT NULL,
  `unit_type` enum('tent','cabin','yurt','treehouse') NOT NULL,
  `capacity` int NOT NULL,
  `price_per_night` decimal(10,2) NOT NULL,
  `description` text,
  `photo` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`unit_id`)
) ENGINE=InnoDB AUTO_INCREMENT=51 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `glampingunits`
--

LOCK TABLES `glampingunits` WRITE;
/*!40000 ALTER TABLE `glampingunits` DISABLE KEYS */;
INSERT INTO `glampingunits` VALUES (1,'Luxury Tent 1','tent',2,150.00,'Luxury tent with private bath','tent.png'),(2,'Luxury Tent 2','tent',2,160.00,'Luxury tent with forest view','pic1.jpg'),(3,'Cozy Cabin 1','cabin',4,200.00,'Cozy wooden cabin','cabin.png'),(4,'Cozy Cabin 2','cabin',4,210.00,'Spacious cabin with fireplace','pic2.jpg'),(5,'Yurt Retreat 1','yurt',3,175.00,'Yurt with mountain view','yurt.png'),(6,'Yurt Retreat 2','yurt',3,185.00,'Traditional yurt near the river','pic3.jpg'),(7,'Treehouse Escape 1','treehouse',2,300.00,'Treehouse in the forest','i.png'),(8,'Treehouse Escape 2','treehouse',2,320.00,'Romantic treehouse with balcony','pic4.jpg'),(9,'Luxury Tent 3','tent',2,155.00,'Tent with king-size bed and private deck','pic5.jpg'),(10,'Cozy Cabin 3','cabin',5,220.00,'Large cabin for families','pic6.jpg'),(11,'Yurt Experience 1','yurt',3,170.00,'Traditional yurt in the forest','pic7.jpg'),(12,'Yurt Experience 2','yurt',4,195.00,'Large yurt with family amenities','pic8.jpg'),(13,'Luxury Tent 4','tent',2,165.00,'Tent with private hot tub','pic9.jpg'),(14,'Luxury Tent 5','tent',2,175.00,'Deluxe tent with mountain view','pic10.jpg'),(15,'Cozy Cabin 4','cabin',4,210.00,'Cabin with fireplace and lake view','pic11.jpg'),(16,'Treehouse Hideaway 1','treehouse',2,310.00,'Treehouse with ocean view','pic12.jpg'),(17,'Treehouse Hideaway 2','treehouse',2,330.00,'Treehouse with hanging bridge','pic13.jpg'),(18,'Yurt Adventure 1','yurt',3,180.00,'Yurt near waterfall','pic14.jpg'),(19,'Luxury Tent 6','tent',2,160.00,'Tent with outdoor shower','pic15.jpg'),(20,'Luxury Tent 7','tent',2,170.00,'Tent with private garden','pic16.jpg'),(21,'Cozy Cabin 5','cabin',4,215.00,'Cabin with hot tub','pic17.jpg'),(22,'Treehouse Retreat 1','treehouse',2,290.00,'Treehouse with forest view','pic18.jpg'),(23,'Yurt Experience 3','yurt',3,175.00,'Yurt with panoramic windows','pic19.jpg'),(24,'Luxury Tent 8','tent',2,155.00,'Tent with skyview','pic20.jpg'),(25,'Luxury Tent 9','tent',2,160.00,'Tent with luxury furnishings','pic21.jpg'),(26,'Cozy Cabin 6','cabin',5,225.00,'Cabin with barbecue area','pic22.jpg'),(27,'Yurt Retreat 3','yurt',4,190.00,'Family yurt near lake','pic23.jpg'),(28,'Treehouse Sanctuary 1','treehouse',2,315.00,'Treehouse with rope swing','pic24.jpg'),(29,'Luxury Tent 10','tent',2,170.00,'Tent with private balcony','pic25.jpg'),(30,'Luxury Tent 11','tent',2,165.00,'Tent with private hammock','pic26.jpg'),(31,'Cozy Cabin 7','cabin',4,230.00,'Cabin with pool','pic27.jpg'),(32,'Yurt Experience 4','yurt',3,180.00,'Yurt with hot springs nearby','pic28.jpg'),(33,'Luxury Tent 12','tent',2,175.00,'Tent with stargazing roof','pic29.jpg'),(34,'Luxury Tent 13','tent',2,160.00,'Tent with waterfall view','pic30.jpg'),(35,'Cozy Cabin 8','cabin',6,240.00,'Large cabin with two bedrooms','pic31.jpg'),(36,'Yurt Adventure 2','yurt',3,185.00,'Yurt near hiking trails','pic32.jpg'),(37,'Treehouse Getaway 1','treehouse',2,325.00,'Treehouse with full kitchen','pic33.jpg'),(38,'Luxury Tent 14','tent',2,165.00,'Tent with outdoor bath','pic2.jpg'),(39,'Cozy Cabin 9','cabin',4,220.00,'Cabin with wood-burning stove','cabin.png'),(40,'Yurt Experience 5','yurt',3,175.00,'Yurt with glass ceiling','yurt.png'),(41,'Luxury Tent 15','tent',2,170.00,'Tent with forest view','tent.png'),(42,'Treehouse Hideaway 3','treehouse',2,320.00,'Treehouse with sun deck','i.png'),(43,'Cozy Cabin 10','cabin',5,235.00,'Cabin with private pool','pic3.jpg'),(44,'Yurt Retreat 4','yurt',4,195.00,'Luxury yurt near mountains','pic5.jpg'),(45,'Luxury Tent 16','tent',2,180.00,'Deluxe tent with hot tub','pic8.jpg'),(46,'Cozy Cabin 11','cabin',4,215.00,'Cabin with private sauna','pic8.jpg'),(47,'Treehouse Sanctuary 2','treehouse',2,340.00,'Treehouse with glass floors','pic4.jpg'),(48,'Yurt Adventure 3','yurt',3,180.00,'Yurt near river','pic10.jpg'),(49,'Luxury Tent 17','tent',2,165.00,'Tent with mountain views','pic12.jpg'),(50,'кемкмк','cabin',1,232323.00,'акумкиенткиемуасвцыукмеирте  кеукпааму ','pic32.jpg');
/*!40000 ALTER TABLE `glampingunits` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `guests`
--

DROP TABLE IF EXISTS `guests`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `guests` (
  `guest_id` int NOT NULL AUTO_INCREMENT,
  `first_name` varchar(50) NOT NULL,
  `last_name` varchar(50) NOT NULL,
  `email` varchar(100) NOT NULL,
  `phone` varchar(20) DEFAULT NULL,
  `date_of_birth` date DEFAULT NULL,
  `passport_number` varchar(20) DEFAULT NULL,
  `registration_date` date DEFAULT NULL,
  PRIMARY KEY (`guest_id`),
  UNIQUE KEY `email` (`email`)
) ENGINE=InnoDB AUTO_INCREMENT=43 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `guests`
--

LOCK TABLES `guests` WRITE;
/*!40000 ALTER TABLE `guests` DISABLE KEYS */;
INSERT INTO `guests` VALUES (1,'Даниил','Даниил','ivan.ivanov@example.com','89012345678','1985-05-15','AB1234567','2024-01-01'),(2,'Мария','Петрова','maria.petrova@example.com','89123456789','1990-07-22','CD2345678','2024-01-02'),(3,'Александр','Сидоров','alex.sidorov@example.com','89234567890','1983-11-12','EF3456789','2024-01-03'),(4,'Екатерина','Кузнецова','katya.kuznetsova@example.com','89345678901','1992-04-10','GH4567890','2024-01-04'),(5,'Дмитрий','Федоров','dmitry.fedorov@example.com','89456789012','1987-06-05','IJ5678901','2024-01-05'),(6,'Ольга','Михайлова','olga.mikhailova@example.com','89567890123','1995-03-19','KL6789012','2024-01-06'),(7,'Павел','Новиков','pavel.novikov@example.com','89678901234','1989-08-17','MN7890123','2024-01-07'),(8,'Анна','Коваленко','anna.kovalenko@example.com','89789012345','1991-12-02','OP8901234','2024-01-08'),(9,'Максим','Зайцев','maksim.zaicev@example.com','89890123456','1986-10-29','QR9012345','2024-01-09'),(10,'Наталья','Гордеева','natasha.gordeeva@example.com','89901234567','1993-01-14','ST0123456','2024-01-10'),(11,'Артем','Морозов','artem.morozov@example.com','89023456789','1994-02-20','UV1234567','2024-01-11'),(12,'Юлия','Васильева','yulia.vasilyeva@example.com','89134567890','1988-09-11','WX2345678','2024-01-12'),(13,'Игорь','Волков','igor.volkov@example.com','89245678901','1985-05-28','YZ3456789','2024-01-13'),(14,'Елена','Лебедева','elena.lebedeva@example.com','89356789012','1990-10-09','AB1237890','2024-01-14'),(15,'Николай','Смирнов','nikolay.smirnov@example.com','89467890123','1987-04-17','CD2348901','2024-01-15'),(16,'Светлана','Кириллова','svetlana.kirillova@example.com','89578901234','1992-12-13','EF3459012','2024-01-16'),(17,'Владимир','Макаров','vladimir.makarov@example.com','89689012345','1986-07-21','GH4560123','2024-01-17'),(18,'Марина','Тимофеева','marina.timofeeva@example.com','89790123456','1993-02-18','IJ5671234','2024-01-18'),(19,'Андрей','Романов','andrey.romanov@example.com','89801234567','1991-09-05','KL6782345','2024-01-19'),(20,'Оксана','Селезнева','oksana.selezneva@example.com','89912345678','1989-06-01','MN7893456','2024-01-20'),(21,'Константин','Лукичев','konstantin.lukichev@example.com','89023456789','1985-03-23','OP8904567','2024-01-21'),(22,'Ирина','Титова','irina.titova@example.com','89134567890','1990-11-25','QR9015678','2024-01-22'),(23,'Сергей','Сергеев','sergey.sergeev@example.com','89245678901','1993-05-16','ST0126789','2024-01-23'),(24,'Алексей','Климов','alexey.klimov@example.com','89356789012','1987-07-07','UV1237890','2024-01-24'),(25,'Галина','Фролова','galina.frolova@example.com','89467890123','1991-03-12','WX2348901','2024-01-25'),(26,'Виктор','Егоров','viktor.egorov@example.com','89578901234','1986-02-27','YZ3459012','2024-01-26'),(27,'Татьяна','Николаева','tatiana.nikolaeva@example.com','89689012345','1994-10-19','AB1230123','2024-01-27'),(28,'Федор','Попов','fedor.popov@example.com','89790123456','1992-06-24','CD2341234','2024-01-28'),(29,'Ксения','Ильина','ksenia.ilina@example.com','89801234567','1989-01-30','EF3452345','2024-01-29'),(30,'Александр','Гончаров','alexander.goncharov@example.com','89912345678','1985-09-18','GH4563456','2024-01-30'),(31,'Лидия','Шишкина','lidia.shishkina@example.com','89023456789','1990-02-03','IJ5674567','2024-01-31'),(32,'Максим','Рожков','maksim.rozhkov@example.com','89134567890','1993-07-14','KL6785678','2024-02-01'),(33,'Дарья','Прохорова','daria.prohorova@example.com','89245678901','1986-08-22','MN7896789','2024-02-02'),(34,'Илья','Панкратов','ilya.pankratov@example.com','89356789012','1989-12-11','OP8907890','2024-02-03'),(35,'Алена','Косарева','alena.kosareva@example.com','89467890123','1994-05-08','QR9018901','2024-02-04'),(36,'Геннадий','Лазарев','gennady.lazarev@example.com','89578901234','1987-11-28','ST0129012','2024-02-05'),(37,'Лариса','Воронцова','larisa.voroncova@example.com','89689012345','1992-09-09','UV1230123','2024-02-06');
/*!40000 ALTER TABLE `guests` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pay_status`
--

DROP TABLE IF EXISTS `pay_status`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pay_status` (
  `idpay_status` int NOT NULL AUTO_INCREMENT,
  `pay_statuscol` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`idpay_status`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pay_status`
--

LOCK TABLES `pay_status` WRITE;
/*!40000 ALTER TABLE `pay_status` DISABLE KEYS */;
INSERT INTO `pay_status` VALUES (1,'Оплаченный'),(2,'Предопалата');
/*!40000 ALTER TABLE `pay_status` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-06-04 14:02:01
