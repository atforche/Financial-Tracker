import type { Breadcrumb } from "@/framework/Breadcrumbs";
import routes from "./routes";

interface NamedEntity {
  readonly id: string;
  readonly name: string;
}

const accountingPeriodsIndex = function (): Breadcrumb[] {
  return [
    {
      label: "Accounting Periods",
      href: routes.accountingPeriods.index,
    },
  ];
};

const accountingPeriodDetail = function (
  accountingPeriod: NamedEntity,
): Breadcrumb[] {
  return [
    ...accountingPeriodsIndex(),
    {
      label: accountingPeriod.name,
      href: routes.accountingPeriods.detail(accountingPeriod.id),
    },
  ];
};

const accountingPeriodFundDetail = function (
  accountingPeriod: NamedEntity,
  fund: NamedEntity,
): Breadcrumb[] {
  return [
    ...accountingPeriodDetail(accountingPeriod),
    {
      label: fund.name,
      href: routes.accountingPeriods.fundDetail(accountingPeriod.id, fund.id),
    },
  ];
};

const accountsIndex = function (): Breadcrumb[] {
  return [
    {
      label: "Accounts",
      href: routes.accounts.index,
    },
  ];
};

const fundsIndex = function (): Breadcrumb[] {
  return [
    {
      label: "Funds",
      href: routes.funds.index,
    },
  ];
};

const fundDetail = function (fund: NamedEntity): Breadcrumb[] {
  return [
    ...fundsIndex(),
    {
      label: fund.name,
      href: routes.funds.detail(fund.id),
    },
  ];
};

const routeBreadcrumbs = {
  accountingPeriods: {
    index: accountingPeriodsIndex,
    create(): Breadcrumb[] {
      return [
        ...accountingPeriodsIndex(),
        {
          label: "Create",
          href: routes.accountingPeriods.create,
        },
      ];
    },
    detail: accountingPeriodDetail,
    close(accountingPeriod: NamedEntity): Breadcrumb[] {
      return [
        ...accountingPeriodDetail(accountingPeriod),
        {
          label: "Close",
          href: routes.accountingPeriods.close(accountingPeriod.id),
        },
      ];
    },
    reopen(accountingPeriod: NamedEntity): Breadcrumb[] {
      return [
        ...accountingPeriodDetail(accountingPeriod),
        {
          label: "Reopen",
          href: routes.accountingPeriods.reopen(accountingPeriod.id),
        },
      ];
    },
    delete(accountingPeriod: NamedEntity): Breadcrumb[] {
      return [
        ...accountingPeriodDetail(accountingPeriod),
        {
          label: "Delete",
          href: routes.accountingPeriods.delete(accountingPeriod.id),
        },
      ];
    },
    fundDetail: accountingPeriodFundDetail,
    fundGoalCreate(
      accountingPeriod: NamedEntity,
      fund: NamedEntity,
    ): Breadcrumb[] {
      return [
        ...accountingPeriodFundDetail(accountingPeriod, fund),
        {
          label: "Add Goal",
          href: routes.accountingPeriods.fundGoalCreate(
            accountingPeriod.id,
            fund.id,
          ),
        },
      ];
    },
    fundGoalUpdate(
      accountingPeriod: NamedEntity,
      fund: NamedEntity,
    ): Breadcrumb[] {
      return [
        ...accountingPeriodFundDetail(accountingPeriod, fund),
        {
          label: "Update Goal",
          href: routes.accountingPeriods.fundGoalUpdate(
            accountingPeriod.id,
            fund.id,
          ),
        },
      ];
    },
    fundGoalDelete(
      accountingPeriod: NamedEntity,
      fund: NamedEntity,
    ): Breadcrumb[] {
      return [
        ...accountingPeriodFundDetail(accountingPeriod, fund),
        {
          label: "Delete Goal",
          href: routes.accountingPeriods.fundGoalDelete(
            accountingPeriod.id,
            fund.id,
          ),
        },
      ];
    },
  },
  accounts: {
    index: accountsIndex,
    update(
      account: NamedEntity,
      accountingPeriod: NamedEntity | null = null,
    ): Breadcrumb[] {
      if (accountingPeriod !== null) {
        return [
          ...accountingPeriodDetail(accountingPeriod),
          {
            label: `Update ${account.name}`,
            href: routes.accounts.update(account.id),
          },
        ];
      }

      return [
        ...accountsIndex(),
        {
          label: `Update ${account.name}`,
          href: routes.accounts.update(account.id),
        },
      ];
    },
    delete(
      account: NamedEntity,
      accountingPeriod: NamedEntity | null = null,
    ): Breadcrumb[] {
      if (accountingPeriod !== null) {
        return [
          ...accountingPeriodDetail(accountingPeriod),
          {
            label: `Delete ${account.name}`,
            href: routes.accounts.delete(account.id),
          },
        ];
      }

      return [
        ...accountsIndex(),
        {
          label: `Delete ${account.name}`,
          href: routes.accounts.delete(account.id),
        },
      ];
    },
  },
  funds: {
    index: fundsIndex,
    create(accountingPeriod: NamedEntity | null = null): Breadcrumb[] {
      if (accountingPeriod !== null) {
        return [
          ...accountingPeriodDetail(accountingPeriod),
          {
            label: "Create Fund",
            href: routes.funds.create,
          },
        ];
      }

      return [
        ...fundsIndex(),
        {
          label: "Create",
          href: routes.funds.create,
        },
      ];
    },
    detail: fundDetail,
    update(
      fund: NamedEntity,
      accountingPeriod: NamedEntity | null = null,
    ): Breadcrumb[] {
      if (accountingPeriod !== null) {
        return [
          ...accountingPeriodFundDetail(accountingPeriod, fund),
          {
            label: "Update",
            href: routes.funds.update(fund.id),
          },
        ];
      }

      return [
        ...fundDetail(fund),
        {
          label: "Update",
          href: routes.funds.update(fund.id),
        },
      ];
    },
    delete(
      fund: NamedEntity,
      accountingPeriod: NamedEntity | null = null,
    ): Breadcrumb[] {
      if (accountingPeriod !== null) {
        return [
          ...accountingPeriodFundDetail(accountingPeriod, fund),
          {
            label: "Delete",
            href: routes.funds.delete(fund.id),
          },
        ];
      }

      return [
        ...fundDetail(fund),
        {
          label: "Delete",
          href: routes.funds.delete(fund.id),
        },
      ];
    },
  },
} as const;

export default routeBreadcrumbs;
