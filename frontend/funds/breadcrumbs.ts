import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Fund } from "@/funds/types";
import accountingPeriodBreadcrumbs from "@/accounting-periods/breadcrumbs";
import routes from "@/funds/routes";

/**
 * Breadcrumbs related to funds.
 */
const breadcrumbs = {
  index: (): Breadcrumb[] => [
    {
      label: "Funds",
      href: routes.index({}),
    },
  ],
  create: (accountingPeriod: AccountingPeriod | null): Breadcrumb[] => {
    if (accountingPeriod !== null) {
      return [
        ...accountingPeriodBreadcrumbs.detail(accountingPeriod),
        {
          label: "Create Fund",
          href: routes.create({}),
        },
      ];
    }
    return [
      ...breadcrumbs.index(),
      {
        label: "Create Fund",
        href: routes.create({}),
      },
    ];
  },
  onboard: (): Breadcrumb[] => [
    ...breadcrumbs.index(),
    {
      label: "Onboard Fund",
      href: routes.onboard,
    },
  ],
  detail: (fund: Fund): Breadcrumb[] => [
    ...breadcrumbs.index(),
    {
      label: fund.name,
      href: routes.detail({ id: fund.id }, {}),
    },
  ],
  update: (
    fund: Fund,
    accountingPeriod: AccountingPeriod | null,
  ): Breadcrumb[] => {
    if (accountingPeriod !== null) {
      return [
        ...accountingPeriodBreadcrumbs.fundDetail(accountingPeriod, fund),
        {
          label: "Update",
          href: routes.update({ id: fund.id }, {}),
        },
      ];
    }
    return [
      ...breadcrumbs.detail(fund),
      {
        label: "Update",
        href: routes.update({ id: fund.id }, {}),
      },
    ];
  },
  delete: (
    fund: Fund,
    accountingPeriod: AccountingPeriod | null,
  ): Breadcrumb[] => {
    if (accountingPeriod !== null) {
      return [
        ...accountingPeriodBreadcrumbs.fundDetail(accountingPeriod, fund),
        {
          label: "Delete",
          href: routes.delete({ id: fund.id }, {}),
        },
      ];
    }
    return [
      ...breadcrumbs.detail(fund),
      {
        label: "Delete",
        href: routes.delete({ id: fund.id }, {}),
      },
    ];
  },
};

export default breadcrumbs;
