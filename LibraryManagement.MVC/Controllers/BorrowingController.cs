using LibraryManagement.BLL.Interfaces;
using LibraryManagement.DAL.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.MVC.Controllers;

public class BorrowingController : Controller
{
    private readonly IBorrowingService _borrowingService;
    private readonly IBookService _bookService;

    public BorrowingController(IBorrowingService borrowingService, IBookService bookService)
    {
        _borrowingService = borrowingService;
        _bookService = bookService;
    }

    // GET: Borrowing
    public async Task<IActionResult> Index()
    {
        var books = await _bookService.GetAllBooks();
        var bookIds = books.Select(b => b.Id);
        var borrowedCounts = await _borrowingService.GetBorrowedCopiesForBooksAsync(bookIds);
        
        ViewBag.BorrowedCounts = borrowedCounts;
        return View(books);
    }

    // POST: Borrowing/Borrow/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Borrow(int id)
    {
        var success = await _borrowingService.BorrowBookAsync(id);
        TempData["Message"] = success ? "Book borrowed successfully!" : "No copies available.";
        TempData["MessageType"] = success ? "success" : "error";
        return RedirectToAction(nameof(Index));
    }

    // POST: Borrowing/Return/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(int id)
    {
        var success = await _borrowingService.ReturnBookAsync(id);
        TempData["Message"] = success ? "Book returned successfully!" : "No borrowed copies to return.";
        TempData["MessageType"] = success ? "success" : "error";
        return RedirectToAction(nameof(Index));
    }

    // GET: Borrowing/History
    public async Task<IActionResult> History(int page = 1, int pageSize = 6)
    {
        try
        {
            // Validate page parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 6;

            var history = await _borrowingService.GetHistoryPaginatedAsync(page, pageSize);
            
            // Add pagination info to ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = history.TotalPages;
            ViewBag.TotalCount = history.TotalCount;
            
            // Get currently borrowed count
            var unreturnedTransactions = await _borrowingService.GetUnreturnedTransactionsAsync();
            ViewBag.CurrentlyBorrowedCount = unreturnedTransactions.Count();
            
            return View(history);
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error loading history: " + ex.Message;
            TempData["MessageType"] = "error";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Borrowing/BorrowAjax
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BorrowAjax(int id)
    {
        try
        {
            Console.WriteLine($"BorrowAjax called with id: {id}");
            var success = await _borrowingService.BorrowBookAsync(id);
            Console.WriteLine($"Borrow result: {success}");
            return Json(new { success = success, message = success ? "Book borrowed successfully!" : "No copies available." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"BorrowAjax error: {ex.Message}");
            return Json(new { success = false, message = "Error borrowing book: " + ex.Message });
        }
    }

    // POST: Borrowing/ReturnAjax
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnAjax(int id)
    {
        try
        {
            Console.WriteLine($"ReturnAjax called with id: {id}");
            var success = await _borrowingService.ReturnBookAsync(id);
            Console.WriteLine($"Return result: {success}");
            return Json(new { success = success, message = success ? "Book returned successfully!" : "No borrowed copies to return." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ReturnAjax error: {ex.Message}");
            return Json(new { success = false, message = "Error returning book: " + ex.Message });
        }
    }

    // GET: Borrowing/CheckUnreturned/5
    [HttpGet]
    public async Task<IActionResult> CheckUnreturned(int id)
    {
        try
        {
            var unreturnedTransactions = await _borrowingService.GetUnreturnedTransactionsAsync();
            var hasUnreturned = unreturnedTransactions.Any(t => t.BookId == id);
            return Json(new { hasUnreturned = hasUnreturned });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CheckUnreturned error: {ex.Message}");
            return Json(new { hasUnreturned = false });
        }
    }

    // GET: Borrowing/Archived
    public async Task<IActionResult> Archived()
    {
        try
        {
            var archivedTransactions = await _borrowingService.GetArchivedTransactionsAsync();
            return View(archivedTransactions);
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error loading archived transactions: " + ex.Message;
            TempData["MessageType"] = "error";
            return RedirectToAction(nameof(History));
        }
    }

    // POST: Borrowing/ArchiveTransaction/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchiveTransaction(int id)
    {
        try
        {
            Console.WriteLine($"ArchiveTransaction called with id: {id}");
            var success = await _borrowingService.ArchiveTransactionAsync(id);
            Console.WriteLine($"Archive result: {success}");
            return Json(new { success = success, message = success ? "Transaction archived successfully!" : "Failed to archive transaction." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ArchiveTransaction error: {ex.Message}");
            return Json(new { success = false, message = "Error archiving transaction: " + ex.Message });
        }
    }

    // POST: Borrowing/UnarchiveTransaction/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnarchiveTransaction(int id)
    {
        try
        {
            Console.WriteLine($"UnarchiveTransaction called with id: {id}");
            var success = await _borrowingService.UnarchiveTransactionAsync(id);
            Console.WriteLine($"Unarchive result: {success}");
            return Json(new { success = success, message = success ? "Transaction unarchived successfully!" : "Failed to unarchive transaction." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UnarchiveTransaction error: {ex.Message}");
            return Json(new { success = false, message = "Error unarchiving transaction: " + ex.Message });
        }
    }
}
