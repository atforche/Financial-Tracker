import Breadcrumbs from "@/framework/breadcrumbs";
import type { JSX } from "react";
import { Stack } from "@mui/material";

/**
 * Component that displays the create Accounting Period page.
 */
const CreateAccountingPeriod = function (): JSX.Element {
  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={[
          { label: "Accounting Periods", href: "/accounting-periods" },
          { label: "Create", href: "/accounting-periods/create" },
        ]}
      />
      <p>This is the create accounting period page</p>
    </Stack>
  );
};

export default CreateAccountingPeriod;
