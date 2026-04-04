import type { Account } from "@/data/accountTypes";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Fund } from "@/data/fundTypes";
import type { Transaction } from "@/data/transactionTypes";

/**
 * Gets the breadcrumbs for the Delete Transaction Form page.
 */
const getBreadcrumbs = function (
  transaction: Transaction,
  accountingPeriod: AccountingPeriod | null,
  account: Account | null,
  fund: Fund | null,
): Breadcrumb[] {
  const breadcrumbs: Breadcrumb[] = [];
  const params = new URLSearchParams();

  if (accountingPeriod !== null) {
    params.set("accountingPeriodId", accountingPeriod.id);
    breadcrumbs.push({
      label: "Accounting Periods",
      href: "/accounting-periods",
    });
    breadcrumbs.push({
      label: accountingPeriod.name,
      href: `/accounting-periods/${accountingPeriod.id}`,
    });

    if (account !== null) {
      params.set("accountId", account.id);
      breadcrumbs.push({
        label: account.name,
        href: `/accounting-periods/${accountingPeriod.id}/accounts/${account.id}`,
      });
    } else if (fund !== null) {
      params.set("fundId", fund.id);
      breadcrumbs.push({
        label: fund.name,
        href: `/accounting-periods/${accountingPeriod.id}/funds/${fund.id}`,
      });
    }
  } else if (account !== null) {
    params.set("accountId", account.id);
    breadcrumbs.push({
      label: "Accounts",
      href: "/accounts",
    });
    breadcrumbs.push({
      label: account.name,
      href: `/accounts/${account.id}`,
    });
  } else if (fund !== null) {
    params.set("fundId", fund.id);
    breadcrumbs.push({
      label: "Funds",
      href: "/funds",
    });
    breadcrumbs.push({
      label: fund.name,
      href: `/funds/${fund.id}`,
    });
  }

  const queryString = params.toString();
  breadcrumbs.push({
    label: "Transaction",
    href: `/transactions/${transaction.id}${queryString ? `?${queryString}` : ""}`,
  });
  breadcrumbs.push({
    label: "Delete",
    href: `/transactions/${transaction.id}/delete`,
  });

  return breadcrumbs;
};

export default getBreadcrumbs;
