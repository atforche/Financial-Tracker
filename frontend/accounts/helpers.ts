import { AccountType } from "@/accounts/types";

/**
 * Determines if the provided account type supports tracked fund assignments.
 */
const isTrackedAccountType = function (accountType: AccountType): boolean {
  switch (accountType) {
    case AccountType.Standard:
    case AccountType.CreditCard:
      return true;
    case AccountType.Investment:
    case AccountType.Debt:
    case AccountType.Retirement:
    case AccountType.Escrow:
      return false;
    default:
      return false;
  }
};

/**
 * Determines if the provided account type is a debt account type.
 */
const isDebtAccountType = function (accountType: AccountType): boolean {
  return (
    accountType === AccountType.Debt ||
    accountType === AccountType.CreditCard
  );
};

/**
 * Determines if the provided change in balance is "positive" based on the provided account type.
 */
const isPositiveChangeInBalance = function (
  accountType: AccountType,
  changeInBalance: number,
): boolean {
  if (accountType === AccountType.Debt) {
    return changeInBalance <= 0;
  }
  return changeInBalance >= 0;
};

/**
 * Formats the provided account type into a readable string.
 */
const formatAccountType = function (accountType: AccountType): string {
  switch (accountType) {
    case AccountType.Standard:
      return "Standard";
    case AccountType.CreditCard:
      return "Credit Card";
    case AccountType.Investment:
      return "Investment";
    case AccountType.Debt:
      return "Debt";
    case AccountType.Retirement:
      return "Retirement";
    case AccountType.Escrow:
      return "Escrow";
    default:
      return accountType;
  }
};

export {
  isTrackedAccountType,
  isDebtAccountType,
  isPositiveChangeInBalance,
  formatAccountType,
}