# 🍲 Food Recipes WebApp

A clean and simple ASP.NET Core web application for browsing and managing food recipes. This project showcases modern .NET Core MVC practices with a focus on user-friendly design and organized recipe management.

## 🚀 Features

- 📝 Add, edit and delete recipes
- 🔍 Browse recipes by category
- 📷 Image support for each recipe
- 🔐 Basic authentication system (if included)
- 🎨 User-friendly UI built with Bootstrap

## 🧱 Tech Stack

- **Backend**: ASP.NET Core MVC (.NET 8)
- **Frontend**: Razor Views, Bootstrap 5
- **Database**: SQL Server (via Entity Framework Core)
- **Other**: LibMan for managing client-side libraries

## 📁 Project Structure
YemekTarifleri/ ├── Controllers/ # Handles requests and business logic ├── Models/ # Entity and view models ├── Views/ # Razor view files ├── wwwroot/ # Static files (CSS, JS, images) ├── appsettings.json # Configuration file ├── Startup.cs # App configuration and middleware └── Program.cs # Main entry point

## 🛠️ How to Run

1. Clone the repository:
   ```bash
   git clone https://github.com/engincts/food-recipes-webapp.git
2. Open the project in Visual Studio 2022+

3. Make sure SQL Server is running. Update appsettings.json with your database connection string.

4. Apply migrations (if needed):
Add-Migration InitialCreate
Update-Database
5.Run the project:
F5 or Ctrl + F5

📸 Screenshots
![image](https://github.com/user-attachments/assets/c982756f-d503-4503-8993-03d4ca602f9f)
![image](https://github.com/user-attachments/assets/fc825f45-34e1-40af-996b-3c4e898bbab4)

📜 License
This project is open-source and free to use under the MIT License.
