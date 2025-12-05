Project Prompt: C2C Trip Planner Web Application
1. Project Overview
You are tasked with building a full-stack web application called "C2C Trip Planner." This application is designed for a group of friends or colleagues who make regular monthly trips together. Its primary purpose is to streamline the process of collecting travel availability from each member and automatically, fairly allocating the responsibility of booking tickets for each trip.
The application will have three distinct user roles with different levels of access and functionality:
User: The standard member. They can submit their available travel dates for a given month and view their assigned travel schedule and booking duties.
Allocation Admin: A coordinator role. This user can view all submitted plans, trigger the automated allocation process, manage a global holiday calendar, and view the master schedule for the entire group.
System Admin: The superuser. This user manages the user accounts (add, edit, delete), their roles, and configures system-wide settings that control the application's behavior.
2. Core Concepts & Data Models
The application revolves around the following data structures:
User: Represents a person using the app.
id: Unique number.
name: Full name (string).
email: Email address (string, optional).
role: User, AllocationAdmin, or SystemAdmin (enum).
pin: A 4-digit security PIN (string, handled by the backend).
sendEmail: A boolean indicating if the user should receive email notifications.
Plan: Represents a user's submitted travel availability for a specific month.
userId: The ID of the user who submitted the plan.
userName: The name of the user.
month: The month of the plan (number, 1-12).
year: The year of the plan (number).
selectedDays: An array of day numbers (e.g., [1, 5, 15, 22]) the user is available.
Allocation: Represents a single, finalized trip with an assigned booker.
date: The date of the trip in "YYYY-MM-DD" format.
bookerId: The ID of the user responsible for booking.
bookerName: The name of the booker.
travelers: An array of objects ({ id, name }) for everyone on that trip.
tripType: A string describing the trip, e.g., "Departure from Home" or "Arrival to Home".
SystemSettings: A global configuration object.
departureLabel: The string label for the first type of trip (e.g., "Departure").
arrivalLabel: The string label for the second type of trip (e.g., "Arrival").
tripPrice: The cost of a single trip (number), used for expense calculation.
allocateForCurrentMonth: A boolean. If true, the admin dashboard defaults to managing the current month. If false, it defaults to the next month.
userListViewEnabled: A boolean. If true, the login screen shows a grid of user profiles. If false, it shows a standard login form.
3. Backend API Specification
The frontend will communicate with a backend REST API. All requests and responses use JSON. The base URL for the API is https://localhost:7116/api.
Authentication
POST /login
Body: { "identifier": "string", "pin": "string" }
Response: User object on success.
Users
GET /users: Fetches all users.
Response: User[]
POST /users: Adds a new user.
Body: { "name", "email", "role", "sendEmail", "actorUserId" }
Response: The newly created User object.
PUT /users/{id}: Updates a user's profile or PIN.
Body: { "name", "email", "role", "sendEmail", "newPin", "currentPin", "actorUserId" } (all fields optional).
Response: 204 No Content.
DELETE /users/{id}: Deletes a user.
Response: 204 No Content.
Plans & Plan Updates
GET /plans?year={year}&month={month}: Fetches all submitted plans for a specific month.
Response: Plan[]
POST /plans: Submits or updates a user's plan.
Body: { "userId", "month", "year", "selectedDays" }
Response: 204 No Content.
GET /plan-updates: Fetches a list of users who have updated their plans since the last allocation.
Response: string[] (an array of user names).
Allocations
GET /allocations?year={year}&month={month}: Fetches all generated allocations for a specific month.
Response: Allocation[]
POST /allocations/generate: Triggers the allocation generation process.
Body: { "actorUserId", "year", "month" }
Response: Allocation[] (the newly generated allocations).
Holidays
GET /holidays: Fetches all global holidays.
Response: string[] (array of ISO date strings, e.g., "2024-12-25T00:00:00Z").
PUT /holidays?actorUserId={id}: Updates the list of global holidays.
Body: { "holidayDates": ["string"] }
Response: 204 No Content.
System Settings
GET /settings: Fetches the system settings.
Response: SystemSettings object.
PUT /settings: Updates the system settings.
Body: SystemSettings object plus an actorUserId.
Response: 204 No Content.
4. Frontend Architecture, Styling, and Global Features
Technology Stack: React (v19+), TypeScript, and Tailwind CSS.
Styling:
Use a clean, modern, and professional aesthetic. The primary color palette should be based on Tailwind's slate shades for backgrounds and text.
Use indigo as the primary accent color for buttons, links, and focus rings.
Use emerald for success states and financial information.
Use red for destructive actions (logout, delete) and highlighting holidays.
The main layout should be built upon reusable Card components (white background, rounded corners, shadow).
Global Features:
Loading Overlay: When data is being fetched or processed, display a full-screen, semi-transparent black overlay with a simple, elegant loading animation (e.g., three bouncing white dots).
Success Banner: On successful actions (login, saving data), display a notification banner. It should be emerald green, appear in the top-right corner, slide in, display a success message, and automatically slide out after 3 seconds.
Session Management: After a successful login, the currentUser object must be stored in sessionStorage to maintain the session on page refresh.
5. Page & View Specifications
A. Login Page
Layout: Centered content with a "Welcome!" heading. A footer at the bottom contains a link to an "About" page.
Conditional View (controlled by userListViewEnabled setting):
List View:
Display a responsive grid of user profile cards.
Each card contains an icon (differentiated for User, Allocation Admin, System Admin), the user's name, and their role title.
The cards should have a subtle hover effect (e.g., lift and increased shadow).
At the top-right, include a search input (which expands on click) and a sort button (toggling between A-Z and Z-A).
Clicking a card opens the PIN entry modal.
Form View:
Display a single card containing a login form.
The form has two fields: a text input for "Name or Email" and a 4-digit PIN input component.
PIN Entry Modal:
This appears as a modal dialog over a darkened background.
It displays "Enter PIN for {UserName}".
It contains the 4-digit PIN input component and buttons for "Cancel" and "Enter".
It shows an error message for incorrect PINs.
B. User Dashboard
Layout: A main grid with a large 2-column wide card on the left and a 1-column wide stack of cards on the right.
Header: A persistent header shows the app title, "Welcome, {UserName}", and a "Logout" button. The user's name is a button that navigates to the "Edit Profile" page.
Left Panel (Trip Planning Card):
Title: "Trip Planning for {Month Year}". Includes "< Prev" and "Next >" buttons to navigate months.
Calendar: A full monthly calendar view. The current day is bordered. Holidays are marked with a red background. User's selected days are marked with a solid indigo background. Clicking a day toggles its selection.
Action: A large "Submit Plan for {Month Year}" button is at the bottom.
Right Panel (Schedule & Expenses):
My Travel Schedule Card: Lists all trips assigned to the user for the selected month, sorted by date. Each entry shows the date, trip type (with a colored badge), the booker's name (with a special "You!" badge if it's the current user), and other travelers.
My Monthly Expense Card: Displays the user's total calculated expense for the month in a large, emerald-colored font. It also shows the calculation breakdown (e.g., "5 trips × ₹100.00/trip").
C. Edit Profile Page
Layout: A single, large card centered on the page.
Form:
Contains fields to edit "Full Name" and "Email Address".
A separate section, "Change PIN (Optional)", contains two 4-digit PIN inputs: one for the "Current PIN" and one for the "New PIN". Validation is required to ensure the current PIN is entered if a new one is set.
Actions: "Back to Dashboard" and "Save Changes" buttons.
D. Allocation Admin Dashboard
Layout: A two-panel design. A main content area and a slim, minimizable left sidebar.
Sidebar: When collapsed (default), it shows icons for "Allocations" and "Calendar". When expanded, it shows text labels.
Allocations View:
Controls: A header to select the month and year to view.
Submitted Plans Card: A list of all users who have submitted their availability for the selected month.
Allocation Control Card: A prominent "Allocate Bookings" button. If users have updated plans since the last allocation, a clear warning message must be displayed here.
Master Schedule Card: A detailed view of all generated trips for the month, showing the date, the assigned booker, and all travelers for each trip.
Calendar View (Holiday Management):
Displays a full-page calendar for managing global holidays.
The admin can click dates to toggle them as holidays.
An "Update Calendar" button saves the changes.
E. System Admin Dashboard
Layout: A two-card, side-by-side layout on larger screens.
Left Card (User Management):
A table lists all users with columns for Name, Email, Role, and "Send Email" preference.
An "Add New User" button sits above the table.
Each user row has "Edit" and "Delete" action links. Deleting another user should prompt for confirmation.
Clicking "Add" or "Edit" opens a modal form.
User Form Modal: A modal dialog with fields for Name, Email, Role (dropdown), and a "Send Email" checkbox.
Right Card (System Settings):
A form to edit all fields from the SystemSettings model.
Includes text inputs for labels, a number input for price, and checkboxes for the boolean flags.
A "Save Settings" button at the bottom.
F. About Page
Layout: A large card with a two-panel layout, accessible from the login screen footer.
Left Panel: A vertical navigation menu with tabs for "Product Details", "Development Details", and "Contact Us".
Right Panel: Displays detailed content based on the selected tab, providing a comprehensive guide to the app's features, the technology stack used to build it, and contact information. The content should be scrollable if it exceeds the viewport height.
