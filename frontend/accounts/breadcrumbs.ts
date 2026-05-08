import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import accountingPeriodBreadcrumbs from "@/accounting-periods/breadcrumbs";
import routes from "@/accounts/routes";

/**
 * Breadcrumbs related to accounts.
 */
const breadcrumbs = {
  index: (): Breadcrumb[] => [
    {
      label: "Accounts",
      href: routes.index({}),
    },
  ],
  create: (accountingPeriod: AccountingPeriod | null): Breadcrumb[] => {
    if (accountingPeriod !== null) {
      return [
        ...accountingPeriodBreadcrumbs.detail(accountingPeriod),
        {
          label: "Create Account",
          href: routes.create({}),
        },
      ];
    }
    return [
      ...breadcrumbs.index(),
      {
        label: "Create Account",
        href: routes.create({}),
      },
    ];
  },
  detail: (account: Account): Breadcrumb[] => [
    ...breadcrumbs.index(),
    {
      label: account.name,
      href: routes.detail({ id: account.id }, {}),
    },
  ],
  update: (
    account: Account,
    accountingPeriod: AccountingPeriod | null,
  ): Breadcrumb[] => {
    if (accountingPeriod !== null) {
      return [
        ...accountingPeriodBreadcrumbs.accountDetail(accountingPeriod, account),
        {
          label: `Update ${account.name}`,
          href: routes.update({ id: account.id }, {}),
        },
      ];
    }
    return [
      ...breadcrumbs.detail(account),
      {
        label: `Update ${account.name}`,
        href: routes.update({ id: account.id }, {}),
      },
    ];
  },
  delete: (
    account: Account,
    accountingPeriod: AccountingPeriod | null,
  ): Breadcrumb[] => {
    if (accountingPeriod !== null) {
      return [
        ...accountingPeriodBreadcrumbs.accountDetail(accountingPeriod, account),
        {
          label: `Delete ${account.name}`,
          href: routes.delete({ id: account.id }, {}),
        },
      ];
    }
    return [
      ...breadcrumbs.detail(account),
      {
        label: `Delete ${account.name}`,
        href: routes.delete({ id: account.id }, {}),
      },
    ];
  },
};

export default breadcrumbs;
