import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Fund } from "@/funds/types";
import routes from "@/accounting-periods/routes";

/**
 * Breadcrumbs related to accounting periods.
 */
const breadcrumbs = {
  reopen: (accountingPeriod: AccountingPeriod): Breadcrumb[] => [
    ...breadcrumbs.detail(accountingPeriod),
    {
      label: "Reopen",
      href: routes.reopen({ id: accountingPeriod.id }),
    },
  ],
  accountDetail: (
    accountingPeriod: AccountingPeriod,
    account: Account,
  ): Breadcrumb[] => [
    ...breadcrumbs.detail(accountingPeriod),
    {
      label: account.name,
      href: routes.accountDetail({
        id: accountingPeriod.id,
        accountId: account.id,
      }),
    },
  ],
  fundDetail: (
    accountingPeriod: AccountingPeriod,
    fund: Fund,
  ): Breadcrumb[] => [
    ...breadcrumbs.detail(accountingPeriod),
    {
      label: fund.name,
      href: routes.fundDetail({ id: accountingPeriod.id, fundId: fund.id }, {}),
    },
  ],
};

export default breadcrumbs;
