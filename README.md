# 📚 Library Management System

A modern, full-featured Library Management System built with ASP.NET Core MVC, featuring book management, borrowing transactions, image handling, and comprehensive unit testing.

## 🚀 Features

### 📖 Book Management
- **CRUD Operations**: Create, Read, Update, and Delete books
- **Image Upload**: Support for book cover images with automatic file management
- **Soft Delete**: Books are soft-deleted to preserve data integrity
- **Genre Classification**: Books are categorized by genres
- **Copy Management**: Track multiple copies of the same book

### 🔄 Borrowing System
- **Borrowing Transactions**: Complete borrowing and returning workflow
- **Availability Tracking**: Real-time tracking of available copies
- **Borrowing History**: View complete borrowing transaction history
- **Archived Transactions**: Maintain historical records of all transactions

### 🛡️ Data Integrity
- **Business Rules**: Prevents deletion of books with active borrowings
- **Validation**: Comprehensive input validation and error handling
- **Soft Delete**: Preserves data while maintaining clean user interface

### 🧪 Testing
- **Unit Tests**: Comprehensive test coverage for business logic
- **Mock Dependencies**: Isolated testing using Moq framework
- **Test Scenarios**: Covers happy path, edge cases, and error conditions

## 🏗️ Architecture

### **Clean Architecture Pattern**
```
LibraryManagement/
├── 📁 LibraryManagement.MVC/          # Presentation Layer
├── 📁 LibraryManagement.BLL/          # Business Logic Layer
├── 📁 LibraryManagement.DAL/          # Data Access Layer
└── 📁 LibraryManagement.Tests/        # Test Project
```

### **Technology Stack**
- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Testing**: xUnit with Moq for mocking
- **Logging**: Serilog with file and console output
- **UI**: Bootstrap 5 with responsive design

## 🛠️ Prerequisites

Before running this application, make sure you have:

- **.NET 8.0 SDK** or later
- **SQL Server** (LocalDB, Express, or Developer Edition)
- **Visual Studio 2022** or **Visual Studio Code**

## ⚡ Quick Start

### 1. Clone the Repository
```bash
git clone https://github.com/AhmedRoshdi36/ELocalizeLibrary.git
cd library-management-system
```

### 2. Database Setup
```bash
# Navigate to the DAL project
cd LibraryManagement.DAL

# Run Entity Framework migrations
dotnet ef database update
```

### 3. Run the Application
```bash
# Navigate to the MVC project
cd ../LibraryManagement.MVC

# Run the application
dotnet run
```

### 4. Access the Application
Open your browser and navigate to:
```
https://localhost:7001
```

## 🧪 Running Tests

### Run All Tests
```bash
cd LibraryManagement.Tests
dotnet test
```

### Run Tests with Verbose Output
```bash
dotnet test --verbosity normal
```

### Run Specific Test
```bash
dotnet test --filter "GetAllBooks_ShouldReturnAllBooks"
```

## 📁 Project Structure

### **LibraryManagement.MVC** (Presentation Layer)
```
Controllers/
├── BooksController.cs          # Book management operations
├── BorrowingController.cs      # Borrowing transaction operations
└── HomeController.cs           # Home page and navigation

Views/
├── Books/                      # Book-related views
├── Borrowing/                  # Borrowing-related views
└── Shared/                     # Layout and common views

wwwroot/
├── css/                        # Stylesheets
├── js/                         # JavaScript files
├── images/                     # Uploaded book images
└── lib/                        # Third-party libraries
```

### **LibraryManagement.BLL** (Business Logic Layer)
```
Services/
├── BookService.cs              # Book business logic
├── BorrowingService.cs         # Borrowing business logic
└── ImageService.cs             # Image handling logic

Interfaces/
├── IBookService.cs             # Book service contract
├── IBorrowingService.cs        # Borrowing service contract
└── IImageService.cs            # Image service contract

Models/
├── BookDeleteInfo.cs           # Book deletion information
└── PaginatedList.cs            # Pagination support
```

### **LibraryManagement.DAL** (Data Access Layer)
```
Entities/
├── Book.cs                     # Book entity
├── BorrowingTransaction.cs     # Borrowing transaction entity
└── Genre.cs                    # Genre entity

Repositories/
├── BookRepository.cs           # Book data access
├── BorrowingTransactionRepository.cs  # Borrowing data access
├── GenericRepository.cs        # Generic CRUD operations
└── UnitOfWork.cs               # Transaction management

Persistance/
└── LibraryDbContext.cs         # Entity Framework context
```

## 🔧 Configuration

### Database Connection
Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LibraryManagementSystem;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Logging Configuration
The application uses Serilog for structured logging:
- **Console Output**: Real-time logging during development
- **File Output**: Daily log files in the `logs/` directory
- **Log Retention**: 30 days of log files

## 🎯 Key Features Explained

### **Book Management Workflow**
1. **Add Book**: Upload cover image + book details
2. **Edit Book**: Modify details with optional image update
3. **Delete Book**: Soft delete with borrowing validation
4. **View Books**: Browse with search and filtering

### **Borrowing Workflow**
1. **Borrow Book**: Check availability and create transaction
2. **Return Book**: Complete transaction and update availability
3. **Track History**: View all borrowing activities
4. **Archive**: Maintain historical records

### **Image Management**
- **Automatic Processing**: Images are processed and stored efficiently
- **File Cleanup**: Old images are automatically deleted during updates
- **Path Management**: Consistent file organization in `wwwroot/images/books/`

## 🧪 Testing Strategy

### **Test Coverage**
- **Service Layer**: 100% coverage of business logic
- **Edge Cases**: Validation errors, business rule violations
- **Mock Dependencies**: Isolated testing of service methods
- **Async Operations**: Proper testing of async/await patterns

### **Test Categories**
- **Unit Tests**: Individual method testing
- **Integration Tests**: Service interaction testing
- **Validation Tests**: Input validation and error handling

## 🤝 Contributing

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. **Open** a Pull Request


## 🙏 Acknowledgments

- **Entity Framework Core** for data access
- **Bootstrap** for responsive UI design
- **Serilog** for structured logging
- **xUnit** and **Moq** for testing framework

## 📞 Support



- **Issues**: Create an issue in the GitHub repository
- **Documentation**: Check the inline code documentation
- **Testing**: Run the test suite to verify functionality

---

**Built with ❤️ using ASP.NET Core and modern development practices**
