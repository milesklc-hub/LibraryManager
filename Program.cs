using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/// <summary>
/// Library manager with a simple Book model and three user accounts:
/// - "Librarian (Admin)": can add/remove/search/list books
/// - "Bob" and "Steve": can borrow/checkin/search books
/// No passwords; users are switched with the `switch` command.
/// </summary>
class LibraryManager
{
	private const int MaxBooks = 5;
	private const int MaxBorrowPerUser = 3;

	// Simple book model that records borrower information.
	class Book
	{
		public string Title { get; }
		public string? Borrower { get; set; }

		public bool IsCheckedOut => !string.IsNullOrEmpty(Borrower);

		public Book(string title)
		{
			Title = title;
			Borrower = null;
		}

		public override string ToString() => Title + (IsCheckedOut ? $" (checked out by {Borrower})" : string.Empty);
	}

	static void Main()
	{
		var catalog = new List<Book>(MaxBooks);

		// Accounts with display names
		var accounts = new Dictionary<string, string>
		{
			{ "librarian", "Librarian (Admin)" },
			{ "bob", "Bob" },
			{ "steve", "Steve" }
		};

		Console.Clear();
		Console.WriteLine("=== Welcome to the Library Management System ===\n");

		while (true)
		{
			string selectedUser = ShowMainMenu(accounts);
			if (selectedUser == "exit") break;

			RunUserSession(catalog, selectedUser, accounts);
		}

		Console.WriteLine("\nThank you for using the Library Management System. Goodbye!");
	}

	static void RunUserSession(List<Book> catalog, string currentUser, Dictionary<string, string> accounts)
	{
		string displayName = accounts[currentUser];

		bool returnToMainMenu = false;

		while (!returnToMainMenu)
		{
			if (IsAdmin(currentUser))
			{
				returnToMainMenu = ShowLibrarianMenu(catalog, currentUser, displayName);
			}
			else
			{
				returnToMainMenu = ShowUserMenu(catalog, currentUser, displayName);
			}
		}
	}

	static bool ShowLibrarianMenu(List<Book> catalog, string currentUser, string displayName)
	{
		Console.WriteLine($"\n--- {displayName}'s Menu ---");
		Console.WriteLine("1. Add a book");
		Console.WriteLine("2. Remove a book");
		Console.WriteLine("3. Search for a book");
		Console.WriteLine("4. List all books");
		Console.WriteLine("5. Exit");
		Console.Write("Enter your choice (1-5 or command name): ");

		string input = (Console.ReadLine() ?? string.Empty).Trim().ToLower();
		
		// Parse numeric input or command name
		string command = ParseMenuInput(input, new[] { "add", "remove", "search", "list", "exit" });

		if (command == "add")
		{
			AddBookInteractive(catalog);
			return false;
		}
		else if (command == "remove")
		{
			RemoveBookInteractive(catalog);
			return false;
		}
		else if (command == "search")
		{
			SearchBookInteractive(catalog);
			return false;
		}
		else if (command == "list")
		{
			PrintCatalog(catalog);
			return false;
		}
		else if (command == "exit")
		{
			return HandleExit();
		}
		else
		{
			Console.WriteLine("\nInvalid choice. Please try again.");
			return false;
		}
	}

	static bool ShowUserMenu(List<Book> catalog, string currentUser, string displayName)
	{
		Console.WriteLine($"\n--- {displayName}'s Menu ---");
		Console.WriteLine("1. Borrow a book");
		Console.WriteLine("2. Check in a book");
		Console.WriteLine("3. Search for a book");
		Console.WriteLine("4. Exit");
		Console.Write("Enter your choice (1-4 or command name): ");

		string input = (Console.ReadLine() ?? string.Empty).Trim().ToLower();
		
		// Parse numeric input or command name
		string command = ParseMenuInput(input, new[] { "borrow", "checkin", "search", "exit" });

		if (command == "borrow")
		{
			BorrowBookInteractive(catalog, currentUser);
			return false;
		}
		else if (command == "checkin")
		{
			CheckinBookInteractive(catalog, currentUser);
			return false;
		}
		else if (command == "search")
		{
			SearchBookInteractive(catalog);
			return false;
		}
		else if (command == "exit")
		{
			return HandleExit();
		}
		else
		{
			Console.WriteLine("\nInvalid choice. Please try again.");
			return false;
		}
	}

	static string ParseMenuInput(string input, string[] options)
	{
		// Check if input is numeric
		if (int.TryParse(input, out int choice))
		{
			if (choice >= 1 && choice <= options.Length)
			{
				return options[choice - 1];
			}
		}

		// Check if input matches a command name
		foreach (var option in options)
		{
			if (input == option)
			{
				return option;
			}
		}

		return string.Empty;
	}

	static bool HandleExit()
	{
		Console.WriteLine("\nWhat would you like to do?");
		Console.WriteLine("1. Return to main menu");
		Console.WriteLine("2. Exit the program");
		Console.Write("Enter your choice (1 or 2): ");

		string input = (Console.ReadLine() ?? string.Empty).Trim();
		if (input == "1")
		{
            Console.WriteLine("\nReturning to main menu...");
			return true; // Return from HandleExit, which exits the session loop
		}
		else if (input == "2")
		{
			Console.WriteLine("\nThank you for using the Library Management System. Goodbye!");
			Environment.Exit(0);
			return false; // This line will never be reached
		}
		else
		{
			Console.WriteLine("\nInvalid choice. Returning to user menu...");
			return false; // Return to user menu on invalid input
		}
	}

	static string ShowMainMenu(Dictionary<string, string> accounts)
	{
		Console.WriteLine("\n--- Main Menu ---");
		Console.WriteLine("Select an account:");
		int i = 1;
		foreach (var kvp in accounts)
		{
			Console.WriteLine($"{i}. {kvp.Value}");
			i++;
		}
		Console.WriteLine($"{i}. Exit");
		Console.Write("Enter your choice (1-{0}): ", i);

		string input = (Console.ReadLine() ?? string.Empty).Trim();
		if (int.TryParse(input, out int choice))
		{
			var accountList = new List<string>(accounts.Keys);
			if (choice >= 1 && choice <= accountList.Count)
			{
				string selected = accountList[choice - 1];
				Console.WriteLine($"\nLogged in as: {accounts[selected]}\n");
				return selected;
			}
			else if (choice == accountList.Count + 1)
			{
				return "exit";
			}
		}

		Console.WriteLine("\nInvalid choice. Please try again.");
		return ShowMainMenu(accounts);
	}

	static bool IsAdmin(string username) => string.Equals(username, "librarian", StringComparison.OrdinalIgnoreCase);

	static void AddBookInteractive(List<Book> catalog)
	{
		if (catalog.Count >= MaxBooks)
		{
			Console.WriteLine("\nThe library is full. No more books can be added.");
			return;
		}

		Console.WriteLine("\nEnter the title of the book to add:");
		string title = (Console.ReadLine() ?? string.Empty).Trim();
		if (string.IsNullOrWhiteSpace(title))
		{
			Console.WriteLine("\nNo title entered. Operation cancelled.");
			return;
		}

		if (catalog.Exists(b => string.Equals(b.Title, title, StringComparison.OrdinalIgnoreCase)))
		{
			Console.WriteLine("\nA book with that title already exists in the catalog.");
			return;
		}

		catalog.Add(new Book(title));
		Console.WriteLine($"Added: \"{title}\"");
	}

	static void RemoveBookInteractive(List<Book> catalog)
	{
		if (catalog.Count == 0)
		{
			Console.WriteLine("\nThe library is empty. No books to remove.");
			return;
		}

		Console.WriteLine("\nEnter the title of the book to remove:");
		string title = (Console.ReadLine() ?? string.Empty).Trim();
		if (string.IsNullOrWhiteSpace(title))
		{
			Console.WriteLine("\nNo title entered. Operation cancelled.");
			return;
		}

		int idx = catalog.FindIndex(b => string.Equals(b.Title, title, StringComparison.OrdinalIgnoreCase));
		if (idx < 0)
		{
			Console.WriteLine("\nBook not found.");
			return;
		}

		var book = catalog[idx];
		if (book.IsCheckedOut)
		{
			Console.WriteLine("\nCannot remove a book that is currently checked out.");
			return;
		}

		catalog.RemoveAt(idx);
		Console.WriteLine($"Removed: \"{title}\"");
	}

	static void SearchBookInteractive(List<Book> catalog)
	{
		if (catalog.Count == 0)
		{
			Console.WriteLine("\nThe library is empty. No books to search.");
			return;
		}

		Console.WriteLine("\nEnter at least 3 characters of the book title to search for:");
		string searchTerm = (Console.ReadLine() ?? string.Empty).Trim();
		if (string.IsNullOrWhiteSpace(searchTerm))
		{
			Console.WriteLine("\nNo search term entered. Operation cancelled.");
			return;
		}

		if (searchTerm.Length < 3)
		{
			Console.WriteLine("\nPlease enter at least 3 characters to search.");
			return;
		}

		// Find all books that contain the search term (case-insensitive)
		var matches = catalog.FindAll(b => b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

		if (matches.Count == 0)
		{
			Console.WriteLine($"\nNo books found matching \"{searchTerm}\".");
			return;
		}

		Console.WriteLine($"\nFound {matches.Count} book(s) matching \"{searchTerm}\":");
		for (int i = 0; i < matches.Count; i++)
		{
			var book = matches[i];
			string status = book.IsCheckedOut ? $"Checked out by {book.Borrower}" : "Available";
			Console.WriteLine($"{i + 1}. {book.Title} — {status}");
		}
		Console.WriteLine();
	}

	static void BorrowBookInteractive(List<Book> catalog, string user)
	{
		if (catalog.Count == 0)
		{
			Console.WriteLine("\nThe library is empty. No books to borrow.");
			return;
		}

		int currentBorrowed = GetBorrowedCount(catalog, user);
		if (currentBorrowed >= MaxBorrowPerUser)
		{
			Console.WriteLine($"\nBorrow limit reached. You may borrow up to {MaxBorrowPerUser} books at a time.");
			return;
		}

		Console.WriteLine("\nEnter the title of the book to borrow:");
		string title = (Console.ReadLine() ?? string.Empty).Trim();
		if (string.IsNullOrWhiteSpace(title))
		{
			Console.WriteLine("\nNo title entered. Operation cancelled.");
			return;
		}

		var book = catalog.Find(b => string.Equals(b.Title, title, StringComparison.OrdinalIgnoreCase));
		if (book == null)
		{
			Console.WriteLine("\nBook not found.");
			return;
		}

		if (book.IsCheckedOut)
		{
			Console.WriteLine($"\nBook already checked out by {book.Borrower}.");
			return;
		}

		book.Borrower = user;
		Console.WriteLine($"\n{user} successfully borrowed \"{book.Title}\".");
	}

	static void CheckinBookInteractive(List<Book> catalog, string user)
	{
		if (catalog.Count == 0)
		{
			Console.WriteLine("\nThe library is empty. No books to check in.");
			return;
		}

		Console.WriteLine("\nEnter the title of the book to check in:");
		string title = (Console.ReadLine() ?? string.Empty).Trim();
		if (string.IsNullOrWhiteSpace(title))
		{
			Console.WriteLine("\nNo title entered. Operation cancelled.");
			return;
		}

		var book = catalog.Find(b => string.Equals(b.Title, title, StringComparison.OrdinalIgnoreCase));
		if (book == null)
		{
			Console.WriteLine("\nBook not found.");
			return;
		}

		if (!book.IsCheckedOut)
		{
			Console.WriteLine("\nThat book is not checked out.");
			return;
		}

		// Librarian can check in any book; regular users can only check in books they borrowed.
		if (!IsAdmin(user) && !string.Equals(book.Borrower, user, StringComparison.OrdinalIgnoreCase))
		{
			Console.WriteLine("\nYou cannot check in a book you did not borrow.");
			return;
		}

		book.Borrower = null;
		Console.WriteLine($"\nChecked in: \"{book.Title}\"");
	}

	static void PrintCatalog(List<Book> catalog)
	{
		Console.WriteLine("\nCatalog:");
		if (catalog.Count == 0)
		{
			Console.WriteLine("(empty)");
			return;
		}

		for (int i = 0; i < catalog.Count; i++)
		{
			var b = catalog[i];
			Console.WriteLine($"{i + 1}. {b}");
		}
	}

	static int GetBorrowedCount(List<Book> catalog, string user)
	{
		int count = 0;
		foreach (var b in catalog)
		{
			if (b.IsCheckedOut && string.Equals(b.Borrower, user, StringComparison.OrdinalIgnoreCase)) count++;
		}
		return count;
	}
}