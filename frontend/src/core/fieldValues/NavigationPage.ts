import FieldValue from "@core/fieldValues/FieldValue";

/**
 * NavigationPage field value that represents the different pages that can be navigated to.
 */
class NavigationPage extends FieldValue {
  private readonly _isDataEntry: boolean;

  /**
   * The Overview page displays a dashboard with a high-level overview.
   */
  public static readonly Overview = new NavigationPage("Overview", false);

  /**
   * The AccountDashboard page displays a dashboard highlighting account information.
   */
  public static readonly AccountDashboard = new NavigationPage(
    "Accounts",
    false,
  );

  /**
   * The FundDashboard page displays a dashboard highlighting fund information.
   */
  public static readonly FundDashboard = new NavigationPage("Funds", false);

  /**
   * The AccountEntry page displays a data entry page for entering accounts.
   */
  public static readonly AccountEntry = new NavigationPage("Accounts", true);

  /**
   * The FundEntry page displays a data entry page for entering funds.
   */
  public static readonly FundEntry = new NavigationPage("Funds", true);

  /**
   * Collection of all NavigationPages.
   */
  public static override readonly Collection = [
    NavigationPage.Overview,
    NavigationPage.AccountDashboard,
    NavigationPage.FundDashboard,
    NavigationPage.AccountEntry,
    NavigationPage.FundEntry,
  ];

  /**
   * Constructs a new instance of this class.
   * @param {string} value - Value for this NavigationPage.
   * @param {boolean} isDataEntry - True if this is a data entry navigation page, false otherwise.
   */
  private constructor(value: string, isDataEntry: boolean) {
    super(value);
    this._isDataEntry = isDataEntry;
  }

  /**
   * Gets the IsDataEntry flag for this NavigationPage.
   * @returns {boolean} The IsDataEntry flag for this NavigationPage.
   */
  public get isDataEntry(): boolean {
    return this._isDataEntry;
  }
}

export default NavigationPage;
