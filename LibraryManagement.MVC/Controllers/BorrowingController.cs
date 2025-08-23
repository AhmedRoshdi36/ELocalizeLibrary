using LibraryManagement.BLL.Interfaces;
using LibraryManagement.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.MVC.Controllers;

public class BorrowingController : Controller
{
    private readonly IBorrowingService _borrowingService;
    private readonly IBookService _bookService;
    private readonly ILogger<BorrowingController> _logger;

    public BorrowingController(IBorrowingService borrowingService, IBookService bookService, ILogger<BorrowingController> logger)
    {
        _borrowingService = borrowingService;
        _bookService = bookService;
        _logger = logger;
    }

    // GET: Borrowing
    public async Task<IActionResult> Index()
    {
        _logger.LogDebug("Borrowing Index action called");
        
        var books = await _bookService.GetAllBooks();
        var bookIds = books.Select(b => b.Id);
        var borrowedCounts = await _borrowingService.GetBorrowedCopiesForBooksAsync(bookIds);
        
        ViewBag.BorrowedCounts = borrowedCounts;
        
        _logger.LogDebug("Borrowing Index action completed. Loaded {BookCount} books", books.Count());
        return View(books);
    }

    // POST: Borrowing/Borrow/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Borrow(int id)
    {
        _logger.LogInformation("Borrow action called for bookId: {BookId}", id);
        
        var success = await _borrowingService.BorrowBookAsync(id);
        TempData["Message"] = success ? "Book borrowed successfully!" : "No copies available.";
        TempData["MessageType"] = success ? "success" : "error";
        
        _logger.LogInformation("Borrow action completed for bookId: {BookId}, Success: {Success}", id, success);
        return RedirectToAction(nameof(Index));
    }

    // POST: Borrowing/Return/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(int id)
    {
        _logger.LogInformation("Return action called for bookId: {BookId}", id);
        
        var success = await _borrowingService.ReturnBookAsync(id);
        TempData["Message"] = success ? "Book returned successfully!" : "No borrowed copies to return.";
        TempData["MessageType"] = success ? "success" : "error";
        
        _logger.LogInformation("Return action completed for bookId: {BookId}, Success: {Success}", id, success);
        return RedirectToAction(nameof(Index));
    }

    // GET: Borrowing/History
    public async Task<IActionResult> History(int page = 1, int pageSize = 6)
    {
        try
        {
            _logger.LogDebug("History action called with page: {Page}, pageSize: {PageSize}", page, pageSize);
            
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
            
            _logger.LogDebug("History action completed. Total transactions: {TotalCount}, Current page: {CurrentPage}", 
                history.TotalCount, page);
            
            return View(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in History action");
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
            _logger.LogInformation("BorrowAjax called with id: {BookId}", id);
            var success = await _borrowingService.BorrowBookAsync(id);
            _logger.LogInformation("BorrowAjax result for bookId: {BookId}, Success: {Success}", id, success);
            return Json(new { success = success, message = success ? "Book borrowed successfully!" : "No copies available." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in BorrowAjax for bookId: {BookId}", id);
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
            _logger.LogInformation("ReturnAjax called with id: {BookId}", id);
            var success = await _borrowingService.ReturnBookAsync(id);
            _logger.LogInformation("ReturnAjax result for bookId: {BookId}, Success: {Success}", id, success);
            return Json(new { success = success, message = success ? "Book returned successfully!" : "No borrowed copies to return." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReturnAjax for bookId: {BookId}", id);
            return Json(new { success = false, message = "Error returning book: " + ex.Message });
        }
    }

    // GET: Borrowing/CheckUnreturned/5
    [HttpGet]
    public async Task<IActionResult> CheckUnreturned(int id)
    {
        try
        {
            _logger.LogDebug("CheckUnreturned called for bookId: {BookId}", id);
            var unreturnedTransactions = await _borrowingService.GetUnreturnedTransactionsAsync();
            var hasUnreturned = unreturnedTransactions.Any(t => t.BookId == id);
            _logger.LogDebug("CheckUnreturned result for bookId: {BookId}, HasUnreturned: {HasUnreturned}", id, hasUnreturned);
            return Json(new { hasUnreturned = hasUnreturned });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CheckUnreturned for bookId: {BookId}", id);
            return Json(new { hasUnreturned = false });
        }
    }

    // GET: Borrowing/Archived
    public async Task<IActionResult> Archived()
    {
        try
        {
            _logger.LogDebug("Archived action called");
            var archivedTransactions = await _borrowingService.GetArchivedTransactionsAsync();
            _logger.LogDebug("Archived action completed. Loaded {Count} archived transactions", archivedTransactions.Count());
            return View(archivedTransactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Archived action");
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
            _logger.LogInformation("ArchiveTransaction called with id: {TransactionId}", id);
            var success = await _borrowingService.ArchiveTransactionAsync(id);
            _logger.LogInformation("ArchiveTransaction result for transactionId: {TransactionId}, Success: {Success}", id, success);
            return Json(new { success = success, message = success ? "Transaction archived successfully!" : "Failed to archive transaction." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ArchiveTransaction for transactionId: {TransactionId}", id);
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
            _logger.LogInformation("UnarchiveTransaction called with id: {TransactionId}", id);
            var success = await _borrowingService.UnarchiveTransactionAsync(id);
            _logger.LogInformation("UnarchiveTransaction result for transactionId: {TransactionId}, Success: {Success}", id, success);
            return Json(new { success = success, message = success ? "Transaction unarchived successfully!" : "Failed to unarchive transaction." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UnarchiveTransaction for transactionId: {TransactionId}", id);
            return Json(new { success = false, message = "Error unarchiving transaction: " + ex.Message });
        }
    }

    // GET: Borrowing/CheckInconsistencies
    [HttpGet]
    public async Task<IActionResult> CheckInconsistencies()
    {
        try
        {
            _logger.LogInformation("CheckInconsistencies called");
            var result = await _borrowingService.FixCopyCountInconsistenciesAsync();
            _logger.LogInformation("CheckInconsistencies completed. Success: {Success}", result);
            return Json(new { success = result, message = "Inconsistency check completed. Check console for details." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CheckInconsistencies");
            return Json(new { success = false, message = "Error checking inconsistencies: " + ex.Message });
        }
    }

    // GET: Borrowing/Debug
    [HttpGet]
    public async Task<IActionResult> Debug()
    {
        try
        {
            _logger.LogDebug("Debug action called");
            var books = await _bookService.GetAllBooks();
            var bookIds = books.Select(b => b.Id);
            var borrowedCounts = await _borrowingService.GetBorrowedCopiesForBooksAsync(bookIds);
            
            var debugInfo = books.Select(book => new
            {
                Id = book.Id,
                Title = book.Title,
                BookCopies = book.Copies,
                BorrowedCount = borrowedCounts.GetValueOrDefault(book.Id, 0),
                Available = book.Copies - borrowedCounts.GetValueOrDefault(book.Id, 0)
            }).ToList();
            
            _logger.LogDebug("Debug action completed. Processed {BookCount} books", books.Count());
            return Json(new { success = true, books = debugInfo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Debug action");
            return Json(new { success = false, message = "Error getting debug info: " + ex.Message });
        }
    }
}
