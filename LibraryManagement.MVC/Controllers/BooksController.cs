using LibraryManagement.BLL.Interfaces;
using LibraryManagement.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;


namespace LibraryManagement.MVC.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookService _bookService; 
        private readonly IBorrowingService _borrowingService;

        public BooksController(IBookService bookService, IBorrowingService borrowingService)
        {
            _bookService = bookService;
            _borrowingService = borrowingService;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var books = await _bookService.GetAllBooks();
            var bookIds = books.Select(b => b.Id);
            var borrowedCounts = await _borrowingService.GetBorrowedCopiesForBooksAsync(bookIds);
            
            ViewBag.BorrowedCounts = borrowedCounts;
            return View(books);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookService.GetBookByIdAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Author,ImagePath,Copies,Genre")] Book book, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _bookService.AddBookAsync(book, ImageFile);
                    return RedirectToAction(nameof(Index));
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("ImageFile", ex.Message);
                    return View(book);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while creating the book. Please try again.");
                    return View(book);
                }
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookService.GetBookByIdAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Author,ImagePath,Copies,Genre")] Book book, IFormFile? ImageFile)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _bookService.UpdateBookAsync(book, ImageFile);
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookService.GetBookByIdAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _bookService.DeleteBookAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Books/GetDeleteInfo/5
        [HttpGet]
        public async Task<IActionResult> GetDeleteInfo(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { success = false, message = "Invalid book ID" });
                }

                var deleteInfo = await _bookService.GetBookDeleteInfoAsync(id);
                return Json(new { 
                    success = true, 
                    book = deleteInfo.Book,
                    totalCopies = deleteInfo.TotalCopies,
                    borrowedCopies = deleteInfo.BorrowedCopies,
                    availableCopies = deleteInfo.AvailableCopies,
                    canDeleteSafely = deleteInfo.CanDeleteSafely,
                    hasBorrowedCopies = deleteInfo.HasBorrowedCopies
                });
            }
            catch (KeyNotFoundException)
            {
                return Json(new { success = false, message = "Book not found" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetDeleteInfo error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while getting book information" });
            }
        }

        // POST: Books/DeleteAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { success = false, message = "Invalid book ID" });
                }

                await _bookService.DeleteBookAsync(id);
                return Json(new { success = true, message = "Book deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return Json(new { success = false, message = "Book not found" });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"DeleteAjax error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while deleting the book" });
            }
        }

        // GET: Books/Deleted
        public async Task<IActionResult> Deleted()
        {
            var deletedBooks = await _bookService.GetDeletedBooksAsync();
            return View(deletedBooks);
        }


    }
}
