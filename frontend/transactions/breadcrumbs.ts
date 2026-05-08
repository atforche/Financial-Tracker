import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Fund } from "@/funds/types";
import type { Transaction } from "@/transactions/types";
import accountBreadcrumbs from "@/accounts/breadcrumbs";
import accountingPeriodBreadcrumbs from "@/accounting-periods/breadcrumbs";
import fundBreadcrumbs from "@/funds/breadcrumbs";
import routes from "@/transactions/routes";

/**
 * Breadcrumbs related to transactions.
 */
const breadcrumbs = {
  create: (
    routeAccountingPeriod: AccountingPeriod | null,
    routeAccount: Account | null,
    routeFund: Fund | null,
  ): Breadcrumb[] => {
    const currentCrumb = {
      label: "Create Transaction",
      href: routes.create({}),
    };
    if (routeAccountingPeriod !== null) {
      if (routeAccount !== null) {
        return [
          ...accountingPeriodBreadcrumbs.accountDetail(
            routeAccountingPeriod,
            routeAccount,
          ),
          currentCrumb,
        ];
      }
      if (routeFund !== null) {
        return [
          ...accountingPeriodBreadcrumbs.fundDetail(
            routeAccountingPeriod,
            routeFund,
          ),
          currentCrumb,
        ];
      }
      return [
        ...accountingPeriodBreadcrumbs.detail(routeAccountingPeriod),
        currentCrumb,
      ];
    }
    if (routeAccount !== null) {
      return [...accountBreadcrumbs.detail(routeAccount), currentCrumb];
    }
    if (routeFund !== null) {
      return [...fundBreadcrumbs.detail(routeFund), currentCrumb];
    }
    return [currentCrumb];
  },
  detail: (
    transaction: Transaction,
    routeAccountingPeriod: AccountingPeriod | null,
    routeAccount: Account | null,
    routeFund: Fund | null,
  ): Breadcrumb[] => {
    const currentCrumb = {
      label: "Transaction",
      href: routes.detail({ id: transaction.id }, {}),
    };
    if (routeAccountingPeriod !== null) {
      if (routeAccount !== null) {
        return [
          ...accountingPeriodBreadcrumbs.accountDetail(
            routeAccountingPeriod,
            routeAccount,
          ),
          currentCrumb,
        ];
      }
      if (routeFund !== null) {
        return [
          ...accountingPeriodBreadcrumbs.fundDetail(
            routeAccountingPeriod,
            routeFund,
          ),
          currentCrumb,
        ];
      }
      return [
        ...accountingPeriodBreadcrumbs.detail(routeAccountingPeriod),
        currentCrumb,
      ];
    }
    if (routeAccount !== null) {
      return [...accountBreadcrumbs.detail(routeAccount), currentCrumb];
    }
    if (routeFund !== null) {
      return [...fundBreadcrumbs.detail(routeFund), currentCrumb];
    }
    return [currentCrumb];
  },
  update: (
    transaction: Transaction,
    routeAccountingPeriod: AccountingPeriod | null,
    routeAccount: Account | null,
    routeFund: Fund | null,
  ): Breadcrumb[] => [
    ...breadcrumbs.detail(
      transaction,
      routeAccountingPeriod,
      routeAccount,
      routeFund,
    ),
    {
      label: "Update",
      href: routes.update({ id: transaction.id }, {}),
    },
  ],
  post: (
    transaction: Transaction,
    routeAccountingPeriod: AccountingPeriod | null,
    routeAccount: Account | null,
  ): Breadcrumb[] => [
    ...breadcrumbs.detail(
      transaction,
      routeAccountingPeriod,
      routeAccount,
      null,
    ),
    {
      label: "Post",
      href: routes.post({ id: transaction.id }, {}),
    },
  ],
  unpost: (
    transaction: Transaction,
    routeAccountingPeriod: AccountingPeriod | null,
    routeAccount: Account | null,
  ): Breadcrumb[] => [
    ...breadcrumbs.detail(
      transaction,
      routeAccountingPeriod,
      routeAccount,
      null,
    ),
    {
      label: "Unpost",
      href: routes.unpost({ id: transaction.id }, {}),
    },
  ],
  delete: (
    transaction: Transaction,
    routeAccountingPeriod: AccountingPeriod | null,
    routeAccount: Account | null,
    routeFund: Fund | null,
  ): Breadcrumb[] => [
    ...breadcrumbs.detail(
      transaction,
      routeAccountingPeriod,
      routeAccount,
      routeFund,
    ),
    {
      label: "Delete",
      href: routes.delete({ id: transaction.id }, {}),
    },
  ],
};

export default breadcrumbs;
