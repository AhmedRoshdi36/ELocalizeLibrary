# 🚀 Quick Setup Guide for Recruiters

Welcome! This guide will help you get the Library Management System running in just a few steps.

## ⚡ Quick Start (3 Steps)

### 1. **Prerequisites**
Make sure you have:
- **.NET 8.0 SDK** installed
- **SQL Server LocalDB** (usually comes with Visual Studio)

### 2. **Database Setup**
```bash
# Navigate to the DAL project
cd LibraryManagement.DAL

# Create and seed the database
dotnet ef database update
```

### 3. **Run the Application**
```bash
# Navigate to the MVC project
cd ../LibraryManagement.MVC

# Start the application
dotnet run
```

### 4. **Access the Application**
Open your browser and go to: `https://localhost:7001`

## 📚 Sample Data Included

The application comes with **5 sample books** and **4 borrowing transactions** to demonstrate functionality:

### **Books Available:**
- **C# 12 Programming** (5 copies available)
- **C++ Programming** (3 copies available - 1 borrowed)
- **Data Structures and Algorithms** (2 copies available - 1 borrowed)
- **Pride and Prejudice** (2 copies available)
- **Clean Code** (4 copies available)

### **Borrowing Transactions:**
- Some books are currently borrowed (shows availability tracking)
- Some transactions are completed (shows history)
- One archived transaction (shows archive functionality)

## 🎯 What You Can Test

### **Book Management:**
- ✅ View all books with availability status
- ✅ Create new books with image upload
- ✅ Edit existing books
- ✅ Delete books (with borrowing validation)

### **Borrowing System:**
- ✅ Borrow books (check availability)
- ✅ Return books
- ✅ View borrowing history
- ✅ See archived transactions

### **Features to Explore:**
- 📖 **Book CRUD Operations** - Full create, read, update, delete
- 🔄 **Borrowing Workflow** - Complete borrow/return cycle
- 🖼️ **Image Management** - Upload and manage book covers
- 📊 **Availability Tracking** - Real-time copy availability
- 🗄️ **Transaction History** - Complete borrowing records
- 🧪 **Unit Tests** - Run `dotnet test` to see test coverage

## 🧪 Running Tests
```bash
cd LibraryManagement.Tests
dotnet test
```
**Result:** 41 tests should pass ✅

## 🆘 Need Help?
- Check the main [README.md](README.md) for detailed documentation
- All features are fully functional with the sample data
- The application demonstrates clean architecture and best practices

---

**Ready to explore! The application is fully functional with sample data.** 🎉
