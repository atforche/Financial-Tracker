import { Account, AccountType } from "./Account";

const accounts = [
  new Account("Checking", AccountType.Standard, true),
  new Account("Savings", AccountType.Standard, true),
  new Account("Retirement", AccountType.Investment, true),
  new Account("Loan", AccountType.Debt, true),
  new Account("Old", AccountType.Standard, false),
];

/**
 * Retrieves Accounts that match the provided criteria.
 * @returns {Account[]} An array of Accounts that match the provided criteria.
 */
const getAccounts = function (): Account[] {
  return accounts;
};

export { getAccounts };
