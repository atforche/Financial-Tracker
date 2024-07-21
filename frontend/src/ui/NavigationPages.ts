/**
 * Enum representing the different Dashboard pages to navigate to.
 */
enum DashboardPage {
  Overview = "Overview",
  Accounts = "Accounts Dashboard",
}

/**
 * Enum representing the different Data Entry pages to navigate to.
 */
enum DataEntryPage {
  Accounts = "Accounts",
}

/**
 * Type representing the union of DashboardPage and DataEntryPage.
 */
type NavigationPage = DashboardPage | DataEntryPage;

export { DashboardPage, DataEntryPage, type NavigationPage };
