"use client";

import { Button, Stack, Typography } from "@mui/material";
import { type JSX, useState } from "react";
import ArrowBack from "@mui/icons-material/ArrowBack";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { ExpectedIncomeSource } from "@/accounting-periods/types";
import ExpectedIncomeSourceDeleteDialog from "@/accounting-periods/workspace/ExpectedIncomeSourceDeleteDialog";
import ExpectedIncomeSourcesEditor from "@/accounting-periods/workspace/ExpectedIncomeSourcesEditor";
import ExpectedIncomeTotalsFrame from "@/accounting-periods/workspace/ExpectedIncomeTotalsFrame";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the ExpectedIncomeSourceDetailsDialog component.
 */
interface ExpectedIncomeSourceDetailsDialogProps {
  readonly source: ExpectedIncomeSource;
  readonly accountingPeriodId: string;
  readonly accountingPeriodName: string;
  readonly year: number;
  readonly month: number;
  readonly canManage: boolean;
  readonly backHref: string;
  readonly editHref: string;
}

/**
 * Displays an expected-income source using the transaction detail page layout.
 */
const ExpectedIncomeSourceDetailsDialog = function ({
  source,
  accountingPeriodId,
  accountingPeriodName,
  year,
  month,
  canManage,
  backHref,
  editHref,
}: ExpectedIncomeSourceDetailsDialogProps): JSX.Element {
  const hasWriteAccess = useWriteAccess();
  const canEdit = canManage && hasWriteAccess;
  const [deleteOpen, setDeleteOpen] = useState(false);

  return (
    <PageLayout>
      <Stack spacing={2.5}>
        <Link
          href={backHref}
          style={{ alignSelf: "flex-start", textDecoration: "none" }}
        >
          <Button component="span" startIcon={<ArrowBack />}>
            Back to Workspace
          </Button>
        </Link>
        <Typography variant="h4">{source.name}</Typography>
      </Stack>
      <ConstrainedContent maxWidth={1200}>
        <Stack spacing={3}>
          <ExpectedIncomeTotalsFrame
            sourceName={source.name}
            accountingPeriodName={accountingPeriodName}
            netPerPayment={source.netAmount.total}
            trackedPerPayment={source.netAmount.tracked}
            untrackedPerPayment={source.netAmount.untracked}
            paymentCount={source.expectedDates.length}
            headerContent={
              canEdit ? (
                <Stack direction="row" spacing={1}>
                  <Button
                    component={Link}
                    href={editHref}
                    variant="outlined"
                    size="small"
                  >
                    Edit
                  </Button>
                  <Button
                    color="error"
                    variant="outlined"
                    size="small"
                    onClick={() => {
                      setDeleteOpen(true);
                    }}
                  >
                    Delete
                  </Button>
                </Stack>
              ) : null
            }
          />
          <ExpectedIncomeSourcesEditor
            source={source}
            year={year}
            month={month}
            readOnly
          />
        </Stack>
      </ConstrainedContent>
      <ExpectedIncomeSourceDeleteDialog
        source={source}
        accountingPeriodId={accountingPeriodId}
        redirectUrl={backHref}
        open={deleteOpen}
        onClose={() => {
          setDeleteOpen(false);
        }}
      />
    </PageLayout>
  );
};

export default ExpectedIncomeSourceDetailsDialog;
