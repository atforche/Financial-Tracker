"use client";

import { Box, Button, Stack, Typography } from "@mui/material";
import { type JSX, useState } from "react";
import ArrowBack from "@mui/icons-material/ArrowBack";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import Divider from "@mui/material/Divider";
import type { ExpectedIncomeSource } from "@/accounting-periods/types";
import ExpectedIncomeSourceDeleteDialog from "@/accounting-periods/workspace/ExpectedIncomeSourceDeleteDialog";
import ExpectedIncomeSourceItemSection from "@/accounting-periods/workspace/ExpectedIncomeSourceItemSection";
import Frame from "@/framework/view/Frame";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import StringEntryField from "@/framework/forms/StringEntryField";
import { alpha } from "@mui/material/styles";
import dayjs from "dayjs";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the ExpectedIncomeSourceDetailsDialog component.
 */
interface ExpectedIncomeSourceDetailsDialogProps {
  readonly source: ExpectedIncomeSource;
  readonly accountingPeriodId: string;
  readonly existingSources: ExpectedIncomeSource[];
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
  existingSources,
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
          <Frame
            title="Details"
            color="info"
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
          >
            <Stack spacing={3}>
              <StringEntryField label="Name" value={source.name} />
              <Divider />
              <ExpectedIncomeSourceItemSection
                title="Income Lines"
                description="Add the gross income amounts that make up each expected payment."
                items={source.incomeLines}
              />
              <Divider />
              <ExpectedIncomeSourceItemSection
                title="Deductions"
                description="Add optional deductions withheld before the income is deposited."
                items={source.incomeDeductions}
              />
              <Divider />
              <Stack spacing={1.5}>
                <Stack spacing={0.25}>
                  <Typography variant="subtitle2">
                    Untracked Transfers
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Add amounts expected to remain outside tracked accounts.
                  </Typography>
                </Stack>
                {source.untrackedTransfers.length === 0 ? (
                  <Typography color="text.secondary">
                    No items available.
                  </Typography>
                ) : (
                  <Box
                    sx={{
                      display: "grid",
                      gap: 1.5,
                      gridTemplateColumns: {
                        xs: "1fr",
                        md: "repeat(2, minmax(0, 1fr))",
                      },
                    }}
                  >
                    {source.untrackedTransfers.map((transfer, index) => (
                      <Box
                        key={`${transfer.description}-${index}`}
                        sx={{
                          display: "grid",
                          gap: 1.5,
                          border: "1px solid",
                          borderColor: "divider",
                          borderRadius: 2,
                          p: 1.5,
                          backgroundColor: (theme) =>
                            alpha(theme.palette.info.main, 0.04),
                          gridTemplateColumns: {
                            xs: "1fr",
                            md: "minmax(0, 1.8fr) minmax(180px, 1fr)",
                          },
                          alignItems: "start",
                        }}
                      >
                        <StringEntryField
                          label="Description"
                          value={transfer.description}
                        />
                        <CurrencyEntryField
                          label="Amount"
                          value={transfer.amount}
                        />
                      </Box>
                    ))}
                  </Box>
                )}
              </Stack>
              <Divider />
              <Stack spacing={1.5}>
                <Stack spacing={0.25}>
                  <Typography variant="subtitle2">
                    Expected Payment Dates
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Add the dates on which this source is expected to pay.
                  </Typography>
                </Stack>
                {source.expectedDates.length === 0 ? (
                  <Typography color="text.secondary">
                    No items available.
                  </Typography>
                ) : (
                  <Box
                    sx={{
                      display: "grid",
                      gap: 1.5,
                      gridTemplateColumns:
                        "repeat(auto-fit, minmax(260px, 1fr))",
                    }}
                  >
                    {source.expectedDates.map((date, index) => (
                      <Box
                        key={`${date}-${index}`}
                        sx={{
                          border: "1px solid",
                          borderColor: "divider",
                          borderRadius: 2,
                          p: 1.5,
                          backgroundColor: (theme) =>
                            alpha(theme.palette.info.main, 0.04),
                        }}
                      >
                        <DateEntryField
                          label="Expected Date"
                          value={dayjs(date)}
                        />
                      </Box>
                    ))}
                  </Box>
                )}
              </Stack>
            </Stack>
          </Frame>
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
        existingSources={existingSources}
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
