import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import { Checkbox } from "@mui/material";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnHeaderButton from "@/framework/listframe/ColumnHeaderButton";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the AccountingPeriodListFrame component.
 */
interface AccountingPeriodListFrameProps {
  readonly queryString: string;
}

/**
 * Component that provides a list of Accounting Period and makes the basic create, read, update, and delete
 * operations available on them.
 * @returns JSX element representing a list of Accounting Period with various action buttons.
 */
const AccountingPeriodListFrame = async function ({
  queryString,
}: AccountingPeriodListFrameProps): Promise<JSX.Element> {
  const client = getApiClient();
  const { data } = await client.GET("/accounting-periods", {
    params: {
      query: {
        QueryString: queryString,
      }
    }
  });

  const columns: ColumnDefinition<AccountingPeriod>[] = [
    {
      name: "period",
      headerContent: "Period",
      getBodyContent: (accountingPeriod: AccountingPeriod) =>
        accountingPeriod.name,
    },
    {
      name: "isOpen",
      headerContent: "Is Open",
      getBodyContent: (accountingPeriod: AccountingPeriod) => (
        <Checkbox checked={accountingPeriod.isOpen} />
      ),
    },
    {
      name: "actions",
      headerContent: (
        <ColumnHeaderButton
          label="Add"
          icon={<AddCircleOutline />}
          onClick={() => {}}
        />
      ),
      getBodyContent: (accountingPeriod: AccountingPeriod) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {}}
        />
      ),
      alignment: "right",
    },
  ];

  return (
    <ListFrame<AccountingPeriod>
      columns={columns}
      getId={(accountingPeriod: AccountingPeriod) => accountingPeriod.id}
      data={data?.items ?? null}
    />
  );
};

export default AccountingPeriodListFrame;
