import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Fund } from "@/funds/types";
import type { Goal } from "@/goals/types";
import accountingPeriodBreadcrumbs from "@/accounting-periods/breadcrumbs";
import routes from "@/goals/routes";

/**
 * Breadcrumbs related to goals.
 */
const breadcrumbs = {
  create: (
    accountingPeriod: AccountingPeriod | null,
    fund: Fund | null,
  ): Breadcrumb[] => {
    const createHref = routes.create({
      accountingPeriodId: accountingPeriod?.id ?? null,
      fundId: fund?.id ?? null,
    });
    if (accountingPeriod !== null && fund !== null) {
      return [
        ...accountingPeriodBreadcrumbs.fundDetail(accountingPeriod, fund),
        {
          label: "Create Goal",
          href: createHref,
        },
      ];
    }
    if (accountingPeriod !== null) {
      return [
        ...accountingPeriodBreadcrumbs.detail(accountingPeriod),
        {
          label: "Create Goal",
          href: createHref,
        },
      ];
    }
    throw new Error("Accounting period must be provided to create a goal");
  },
  detail: (accountingPeriod: AccountingPeriod, goal: Goal): Breadcrumb[] => [
    ...accountingPeriodBreadcrumbs.detail(accountingPeriod),
    {
      label: `${goal.fundName} Goal`,
      href: routes.detail({ id: goal.id }, {}),
    },
  ],
  update: (
    accountingPeriod: AccountingPeriod,
    goal: Goal,
    fund: Fund | null,
  ): Breadcrumb[] => {
    const updateHref = routes.update({ id: goal.id }, {});
    if (fund !== null) {
      return [
        ...accountingPeriodBreadcrumbs.fundDetail(accountingPeriod, fund),
        {
          label: "Update Goal",
          href: updateHref,
        },
      ];
    }
    return [
      ...accountingPeriodBreadcrumbs.detail(accountingPeriod),
      {
        label: `Update ${goal.fundName} Goal`,
        href: updateHref,
      },
    ];
  },
  delete: (
    accountingPeriod: AccountingPeriod,
    goal: Goal,
    fund: Fund | null,
  ): Breadcrumb[] => {
    const deleteHref = routes.delete({ id: goal.id }, {});
    if (fund !== null) {
      return [
        ...accountingPeriodBreadcrumbs.fundDetail(accountingPeriod, fund),
        {
          label: "Delete Goal",
          href: deleteHref,
        },
      ];
    }
    return [
      ...accountingPeriodBreadcrumbs.detail(accountingPeriod),
      {
        label: `Delete ${goal.fundName} Goal`,
        href: deleteHref,
      },
    ];
  },
};

export default breadcrumbs;
