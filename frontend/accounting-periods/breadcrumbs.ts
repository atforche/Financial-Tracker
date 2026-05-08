import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Fund } from "@/funds/types";
import routes from "@/accounting-periods/routes";

/**
 * Breadcrumbs related to accounting periods.
 */
const breadcrumbs = {
  index: (): Breadcrumb[] => [
    {
      label: "Accounting Periods",
      href: routes.index({}),
    },
  ],
  create: (): Breadcrumb[] => [
    ...breadcrumbs.index(),
    {
      label: "Create",
      href: routes.create,
    },
  ],
  detail: (accountingPeriod: AccountingPeriod): Breadcrumb[] => [
    ...breadcrumbs.index(),
    {
      label: accountingPeriod.name,
      href: routes.detail({ id: accountingPeriod.id }, {}),
    },
  ],
  close: (accountingPeriod: AccountingPeriod): Breadcrumb[] => [
    ...breadcrumbs.detail(accountingPeriod),
    {
      label: "Close",
      href: routes.close({ id: accountingPeriod.id }),
    },
  ],
  reopen: (accountingPeriod: AccountingPeriod): Breadcrumb[] => [
    ...breadcrumbs.detail(accountingPeriod),
    {
      label: "Reopen",
      href: routes.reopen({ id: accountingPeriod.id }),
    },
  ],
  delete: (accountingPeriod: AccountingPeriod): Breadcrumb[] => [
    ...breadcrumbs.detail(accountingPeriod),
    {
      label: "Delete",
      href: routes.delete({ id: accountingPeriod.id }),
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
