import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Fund } from "@/funds/types";
import type { Goal } from "@/goals/types";
import accountingPeriodBreadcrumbs from "@/accounting-periods/breadcrumbs";
import routes from "@/goals/routes";

/**
 * Returns the goals index breadcrumb when the provided return URL points at the goals workspace.
 */
const getGoalsIndexBreadcrumbs = function (
  returnUrl?: string | null,
): Breadcrumb[] | null {
  if (typeof returnUrl === "string" && returnUrl.startsWith("/goals")) {
    return [{ label: "Goals", href: returnUrl }];
  }
  return null;
};

/**
 * Breadcrumbs related to goals.
 */
const breadcrumbs = {
  index: (): Breadcrumb[] => [
    {
      label: "Goals",
      href: routes.index({}),
    },
  ],
  create: (
    accountingPeriod: AccountingPeriod | null,
    fund: Fund | null,
    returnUrl?: string | null,
  ): Breadcrumb[] => {
    const goalsIndexBreadcrumbs = getGoalsIndexBreadcrumbs(returnUrl);
    const createHref = routes.create({
      accountingPeriodId: accountingPeriod?.id ?? null,
      fundId: fund?.id ?? null,
      returnUrl: returnUrl ?? null,
    });
    if (goalsIndexBreadcrumbs !== null) {
      return [
        ...goalsIndexBreadcrumbs,
        {
          label: "Create Goal",
          href: createHref,
        },
      ];
    }
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
    return [
      ...breadcrumbs.index(),
      {
        label: "Create Goal",
        href: createHref,
      },
    ];
  },
  detail: (
    accountingPeriod: AccountingPeriod,
    goal: Goal,
    returnUrl?: string | null,
  ): Breadcrumb[] => {
    const goalsIndexBreadcrumbs = getGoalsIndexBreadcrumbs(returnUrl);
    const detailCrumb = {
      label: `${goal.fundName} Goal`,
      href: routes.detail({ id: goal.id }, { returnUrl: returnUrl ?? null }),
    };
    if (goalsIndexBreadcrumbs !== null) {
      return [...goalsIndexBreadcrumbs, detailCrumb];
    }
    return [
      ...accountingPeriodBreadcrumbs.detail(accountingPeriod),
      detailCrumb,
    ];
  },
  update: (
    accountingPeriod: AccountingPeriod,
    goal: Goal,
    fund: Fund | null,
    returnUrl?: string | null,
  ): Breadcrumb[] => {
    const goalsIndexBreadcrumbs = getGoalsIndexBreadcrumbs(returnUrl);
    const updateHref = routes.update(
      { id: goal.id },
      { returnUrl: returnUrl ?? null },
    );
    if (goalsIndexBreadcrumbs !== null) {
      return [
        ...breadcrumbs.detail(accountingPeriod, goal, returnUrl),
        {
          label: "Update Goal",
          href: updateHref,
        },
      ];
    }
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
    returnUrl?: string | null,
  ): Breadcrumb[] => {
    const goalsIndexBreadcrumbs = getGoalsIndexBreadcrumbs(returnUrl);
    const deleteHref = routes.delete(
      { id: goal.id },
      { returnUrl: returnUrl ?? null },
    );
    if (goalsIndexBreadcrumbs !== null) {
      return [
        ...breadcrumbs.detail(accountingPeriod, goal, returnUrl),
        {
          label: "Delete Goal",
          href: deleteHref,
        },
      ];
    }
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
