"use client";

import { Button, Stack, Typography } from "@mui/material";
import { type JSX, useState } from "react";
import ArrowBack from "@mui/icons-material/ArrowBack";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { ExpectedIncomeSource } from "@/accounting-periods/types";
import ExpectedIncomeSourceDeleteDialog from "@/accounting-periods/workspace/ExpectedIncomeSourceDeleteDialog";
import ExpectedIncomeSourcesEditor from "@/accounting-periods/workspace/ExpectedIncomeSourcesEditor";
import Frame from "@/framework/view/Frame";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import StringEntryField from "@/framework/forms/StringEntryField";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the ExpectedIncomeSourceDetailsDialog component.
 */
interface ExpectedIncomeSourceDetailsDialogProps {
  readonly source: ExpectedIncomeSource;
  readonly accountingPeriodId: string;
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
        <Typography variant="h4">Expected Income Source</Typography>
      </Stack>
      <ConstrainedContent maxWidth={1200}>
        <Stack spacing={3}>
          <ExpectedIncomeSourcesEditor
            source={source}
            year={year}
            month={month}
            readOnly
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
          <Frame title="Calculated Totals" color="info">
            <ResponsiveGrid columns={{ xs: 1, sm: 3 }} spacing={2}>
              <CurrencyEntryField
                label="Net per payment"
                value={source.netAmount.total}
              />
              <CurrencyEntryField
                label="Tracked per payment"
                value={source.netAmount.tracked}
              />
              <CurrencyEntryField
                label="Untracked per payment"
                value={source.netAmount.untracked}
              />
              <CurrencyEntryField
                label="Expected total"
                value={source.expectedAmount.total}
              />
              <CurrencyEntryField
                label="Expected tracked"
                value={source.expectedAmount.tracked}
              />
              <CurrencyEntryField
                label="Expected untracked"
                value={source.expectedAmount.untracked}
              />
              <StringEntryField
                label="Expected payments"
                value={String(source.expectedDates.length)}
              />
            </ResponsiveGrid>
          </Frame>
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
