import routes, { withQuery } from "./routes";
import type { Breadcrumb } from "@/framework/Breadcrumbs";

interface NamedEntity {
  readonly id: string;
  readonly name: string;
}

interface TransactionNavigationContext {
  readonly accountingPeriod?: NamedEntity | null;
  readonly account?: NamedEntity | null;
  readonly fund?: NamedEntity | null;
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
  display?: "accounts" | "funds" | "transactions",
): Breadcrumb[] {
  return [
    ...accountingPeriodsIndex(),
    {
      label: accountingPeriod.name,
      href: withQuery(routes.accountingPeriods.detail(accountingPeriod.id), {
        display,
      }),
    },
  ];
};

const accountingPeriodAccountDetail = function (
  accountingPeriod: NamedEntity,
  account: NamedEntity,
): Breadcrumb[] {
  return [
    ...accountingPeriodDetail(accountingPeriod, "accounts"),
    {
      label: account.name,
      href: routes.accountingPeriods.accountDetail(
        accountingPeriod.id,
        account.id,
      ),
    },
  ];
};

const accountingPeriodFundDetail = function (
  accountingPeriod: NamedEntity,
  fund: NamedEntity,
): Breadcrumb[] {
  return [
    ...accountingPeriodDetail(accountingPeriod, "funds"),
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

const accountDetail = function (account: NamedEntity): Breadcrumb[] {
  return [
    ...accountsIndex(),
    {
      label: account.name,
      href: routes.accounts.detail(account.id),
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

const transactionDetail = function (
  transactionId: string,
  context: TransactionNavigationContext = {},
): Breadcrumb[] {
  const href = withQuery(routes.transactions.detail(transactionId), {
    accountingPeriodId: context.accountingPeriod?.id,
    accountId: context.account?.id,
    fundId: context.fund?.id,
  });

  if (context.accountingPeriod && context.fund) {
    return [
      ...accountingPeriodFundDetail(context.accountingPeriod, context.fund),
      {
        label: "Transaction",
        href,
      },
    ];
  }

  if (context.accountingPeriod && context.account) {
    return [
      ...accountingPeriodAccountDetail(
        context.accountingPeriod,
        context.account,
      ),
      {
        label: "Transaction",
        href,
      },
    ];
  }

  if (context.account) {
    return [
      ...accountDetail(context.account),
      {
        label: "Transaction",
        href,
      },
    ];
  }

  if (context.fund) {
    return [
      ...fundDetail(context.fund),
      {
        label: "Transaction",
        href,
      },
    ];
  }

  if (context.accountingPeriod) {
    return [
      ...accountingPeriodDetail(context.accountingPeriod, "transactions"),
      {
        label: "Transaction",
        href,
      },
    ];
  }

  return [
    {
      label: "Transaction",
      href,
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
    accountDetail: accountingPeriodAccountDetail,
    fundDetail: accountingPeriodFundDetail,
    goalCreate(accountingPeriod: NamedEntity, fund: NamedEntity): Breadcrumb[] {
      return [
        ...accountingPeriodFundDetail(accountingPeriod, fund),
        {
          label: "Add Goal",
          href: routes.accountingPeriods.goalCreate(
            accountingPeriod.id,
            fund.id,
          ),
        },
      ];
    },
    goalUpdate(accountingPeriod: NamedEntity, fund: NamedEntity): Breadcrumb[] {
      return [
        ...accountingPeriodFundDetail(accountingPeriod, fund),
        {
          label: "Update Goal",
          href: routes.accountingPeriods.goalUpdate(
            accountingPeriod.id,
            fund.id,
          ),
        },
      ];
    },
    goalDelete(accountingPeriod: NamedEntity, fund: NamedEntity): Breadcrumb[] {
      return [
        ...accountingPeriodFundDetail(accountingPeriod, fund),
        {
          label: "Delete Goal",
          href: routes.accountingPeriods.goalDelete(
            accountingPeriod.id,
            fund.id,
          ),
        },
      ];
    },
  },
  accounts: {
    index: accountsIndex,
    create(accountingPeriod: NamedEntity | null = null): Breadcrumb[] {
      if (accountingPeriod !== null) {
        return [
          ...accountingPeriodDetail(accountingPeriod),
          {
            label: "Create Account",
            href: routes.accounts.create,
          },
        ];
      }

      return [
        ...accountsIndex(),
        {
          label: "Create",
          href: routes.accounts.create,
        },
      ];
    },
    detail: accountDetail,
    update(
      account: NamedEntity,
      accountingPeriod: NamedEntity | null = null,
    ): Breadcrumb[] {
      if (accountingPeriod !== null) {
        return [
          ...accountingPeriodAccountDetail(accountingPeriod, account),
          {
            label: `Update ${account.name}`,
            href: routes.accounts.update(account.id),
          },
        ];
      }

      return [
        ...accountDetail(account),
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
          ...accountingPeriodAccountDetail(accountingPeriod, account),
          {
            label: `Delete ${account.name}`,
            href: routes.accounts.delete(account.id),
          },
        ];
      }

      return [
        ...accountDetail(account),
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
  transactions: {
    detail: transactionDetail,
  },
} as const;

export default routeBreadcrumbs;
