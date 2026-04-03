import {
  type CreateTransactionFormSearchParams,
  getDefaultAccountingPeriod,
  getDefaultCreditAccount,
  getDefaultCreditFundAmount,
  getDefaultDebitAccount,
  getDefaultDebitFundAmount,
} from "@/app/transactions/create/createTransactionFormSearchParams";
import type { AccountIdentifier } from "@/data/accountTypes";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { FundIdentifier } from "@/data/fundTypes";

/**
 * Gets the breadcrumbs for the Create Transaction Form page based on the search params.
 */
const getBreadcrumbs = function (
  searchParams: CreateTransactionFormSearchParams,
  accountingPeriods: AccountingPeriod[],
  accounts: AccountIdentifier[],
  funds: FundIdentifier[],
): Breadcrumb[] {
  const breadcrumbs: Breadcrumb[] = [];

  const defaultAccountingPeriod = getDefaultAccountingPeriod(
    accountingPeriods,
    searchParams,
  );
  const defaultAccount =
    getDefaultDebitAccount(accounts, searchParams) ??
    getDefaultCreditAccount(accounts, searchParams);
  let defaultFund = getDefaultDebitFundAmount(funds, searchParams);
  if (defaultFund.length === 0) {
    defaultFund = getDefaultCreditFundAmount(funds, searchParams);
  }

  if (defaultAccountingPeriod !== null) {
    breadcrumbs.push({
      label: "Accounting Periods",
      href: "/accounting-periods",
    });
    breadcrumbs.push({
      label: defaultAccountingPeriod.name,
      href: `/accounting-periods/${defaultAccountingPeriod.id}`,
    });

    if (defaultAccount !== null) {
      breadcrumbs.push({
        label: defaultAccount.name,
        href: `/accounting-periods/${defaultAccountingPeriod.id}/accounts/${defaultAccount.id}`,
      });
    }
    if (typeof defaultFund[0] !== "undefined") {
      breadcrumbs.push({
        label: defaultFund[0].fundName,
        href: `/accounting-periods/${defaultAccountingPeriod.id}/funds/${defaultFund[0].fundId}`,
      });
    }
  } else if (defaultAccount !== null) {
    breadcrumbs.push({
      label: "Accounts",
      href: "/accounts",
    });
    breadcrumbs.push({
      label: defaultAccount.name,
      href: `/accounts/${defaultAccount.id}`,
    });
  } else if (typeof defaultFund[0] !== "undefined") {
    breadcrumbs.push({
      label: "Funds",
      href: "/funds",
    });
    breadcrumbs.push({
      label: defaultFund[0].fundName,
      href: `/funds/${defaultFund[0].fundId}`,
    });
  }

  breadcrumbs.push({
    label: "Create Transaction",
    href: "/transactions/create",
  });

  return breadcrumbs;
};

export default getBreadcrumbs;
