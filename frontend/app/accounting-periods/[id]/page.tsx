import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import getApiClient from "@/data/getApiClient";

/**
 * Component that displays the view for a single accounting period.
 */
const AccountingPeriodView = async function (props: {
  readonly params: Promise<{ id: string }>;
}): Promise<JSX.Element> {
  const { id } = await props.params;

  const apiClient = getApiClient();
  const { data, error } = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: {
        path: {
          accountingPeriodId: id,
        },
      },
    },
  );
  if (typeof data === "undefined") {
    throw new Error(
      `Failed to fetch accounting period with ID ${id}: ${error.detail}`,
    );
  }

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={[
          { label: "Accounting Periods", href: "/accounting-periods" },
          { label: data.name, href: `/accounting-periods/${id}` },
        ]}
      />
      <CaptionedFrame caption="Details">
        <CaptionedValue caption="Name" value={data.name} />
        <CaptionedValue caption="Is Open" value={data.isOpen ? "Yes" : "No"} />
        <CaptionedValue caption="Opening Balance" value="TBD" />
        <CaptionedValue caption="Closing Balance" value="TBD" />
      </CaptionedFrame>
    </Stack>
  );
};

export default AccountingPeriodView;
