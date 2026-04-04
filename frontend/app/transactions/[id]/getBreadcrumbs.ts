import type { Account } from "@/data/accountTypes";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Fund } from "@/data/fundTypes";
import type { Transaction } from "@/data/transactionTypes";

/**
 * Gets the breadcrumbs to be displayed at the top of the page.
 */
const getBreadcrumbs = function (
  transaction: Transaction,
  accountingPeriod: AccountingPeriod | null,
  account: Account | null,
  fund: Fund | null,
): Breadcrumb[] {
  const breadcrumbs: Breadcrumb[] = [];

  if (accountingPeriod !== null) {
    breadcrumbs.push({
      label: "Accounting Periods",
      href: "/accounting-periods",
    });
    breadcrumbs.push({
      label: accountingPeriod.name,
      href: `/accounting-periods/${accountingPeriod.id}`,
    });

    if (account !== null) {
      breadcrumbs.push({
        label: account.name,
        href: `/accounting-periods/${accountingPeriod.id}/accounts/${account.id}`,
      });
    } else if (fund !== null) {
      breadcrumbs.push({
        label: fund.name,
        href: `/accounting-periods/${accountingPeriod.id}/funds/${fund.id}`,
      });
    }
  } else if (account !== null) {
    breadcrumbs.push({
      label: "Accounts",
      href: "/accounts",
    });
    breadcrumbs.push({
      label: account.name,
      href: `/accounts/${account.id}`,
    });
  } else if (fund !== null) {
    breadcrumbs.push({
      label: "Funds",
      href: "/funds",
    });
    breadcrumbs.push({
      label: fund.name,
      href: `/funds/${fund.id}`,
    });
  }

  breadcrumbs.push({
    label: `Transaction`,
    href: `/transactions/${transaction.id}`,
  });

  return breadcrumbs;
};

export default getBreadcrumbs;
