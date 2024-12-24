Library Management System
Overview
The Library Management System (LMS) is a comprehensive platform built using ASP.NET MVC and Entity Framework Core, designed to simplify library operations for both administrators and students. It offers functionalities like book management, user registration, book issue and renewal, fine management, and student activity tracking.

Features
Admin Functionalities
Login: Secure access to the admin dashboard.
Add Books: Add new books to the library collection.
Search Books: Find books by title, author, or ISBN.
Issue Books: Issue books to registered students.
Renew Books: Extend the borrowing period for issued books.
Calculate Fine: Determine fines for overdue books.
Generate Reports: Create reports for inventory and student activities.
Manage Fines: Track and update fine payments.
Logout: End the admin session securely.
Student Functionalities
Home Page Access: View login and registration options.
Register: Sign up for a new student account.
Login: Access the student dashboard after authentication.
Search Books: Search the library collection for books.
Check Book Availability: Verify if specific books are available.
Request Books: Submit requests to borrow books.
View Issued Books: Track borrowed books and their due dates.
Renew Books: Request extensions for borrowed books.
Return Books: Mark books as returned.
View Fines: Check and pay overdue fines.
Logout: End the session securely.
Technologies Used
Frontend: HTML, CSS, JavaScript, Razor Views
Backend: C#, ASP.NET MVC
Database: SQL Server (using Entity Framework Core)
Tools: Visual Studio, SQL Server Management Studio (SSMS)
Setup Instructions
Prerequisites
Install .NET 8 SDK.
Install SQL Server and SQL Server Management Studio (SSMS).
Install Visual Studio 2022 with the ASP.NET workload.
Steps
Clone the Repository:

bash
Copy code
git clone https://github.com/yourusername/library-management-system.git
cd library-management-system
Set Up the Database:

Configure the appsettings.json file with your database connection string.
Run the following commands to create and update the database:
bash
Copy code
dotnet ef migrations add InitialCreate
dotnet ef database update
Run the Application:

Open the project in Visual Studio.
Run the project using IIS Express or Kestrel.
Access the Application:

Navigate to http://localhost:<port> in your browser.
Project Structure
plaintext
Copy code
LibraryManagementSystem/
├── Controllers/
│   ├── AdminController.cs
│   ├── StudentController.cs
│   └── HomeController.cs
├── Models/
│   ├── Book.cs
│   ├── User.cs
│   ├── Fine.cs
│   ├── IssuedBook.cs
│   └── StudentReport.cs
├── Views/
│   ├── Admin/
│   ├── Student/
│   ├── Shared/
│   └── Home/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
└── appsettings.json
Usage
Admin:
Use the admin credentials to log in and manage the library.
Students:
Register and log in to access personalized library functionalities.
Future Enhancements
Implement notifications for overdue books.
Add advanced search filters for books.
Enhance the user interface with modern design elements.
Include support for exporting reports in PDF or Excel.
 

Contributors
Ujwal (Developer)
Feel free to improve and contribute to the project!
