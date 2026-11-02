"use client";

import { Box, Button, Chip, Stack, Typography } from "@mui/material";
import type {
  ExpectedIncomeSource,
  ExpectedIncomeSourceIncome,
} from "@/accounting-periods/types";
import { type JSX, useState } from "react";
import ExpectedIncomePaymentCard from "@/accounting-periods/workspace/ExpectedIncomePaymentCard";
import ExpectedIncomePeriodCard from "@/accounting-periods/workspace/ExpectedIncomePeriodCard";
import ExpectedIncomeReceivedCard from "@/accounting-periods/workspace/ExpectedIncomeReceivedCard";
import ExpectedIncomeSourceDeleteDialog from "@/accounting-periods/workspace/ExpectedIncomeSourceDeleteDialog";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import ReturnLink from "@/framework/view/ReturnLink";
import routes from "@/accounting-periods/routes";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the ExpectedIncomeSourceDetails component.
 */
interface ExpectedIncomeSourceDetailsProps {
  readonly source: ExpectedIncomeSource;
  readonly income: readonly ExpectedIncomeSourceIncome[];
  readonly accountingPeriodId: string;
  readonly accountingPeriodName: string;
  readonly canManage: boolean;
  readonly backHref: string;
  readonly currentUrl: string;
  readonly editHref: string;
}

/**
 * Displays an expected-income source: how much it has received against what it expects, how each payment breaks
 * down, and the income linked to it.
 */
const ExpectedIncomeSourceDetails = function ({
  source,
  income,
  accountingPeriodId,
  accountingPeriodName,
  canManage,
  backHref,
  currentUrl,
  editHref,
}: ExpectedIncomeSourceDetailsProps): JSX.Element {
  const hasWriteAccess = useWriteAccess();
  const canEdit = canManage && hasWriteAccess;
  const [deleteOpen, setDeleteOpen] = useState(false);

  return (
    <PageLayout>
      <Stack spacing={2}>
        <ReturnLink
          returnUrl={backHref}
          fallback={{
            href: routes.workspaceDetail(accountingPeriodId, {}),
            label: `Back to ${accountingPeriodName}`,
          }}
        />
        <Stack
          direction="row"
          useFlexGap
          spacing={2}
          sx={{
            flexWrap: "wrap",
            alignItems: "center",
            justifyContent: "space-between",
          }}
        >
          <Stack
            direction="row"
            useFlexGap
            spacing={1.5}
            sx={{ alignItems: "center", flexWrap: "wrap", minWidth: 0 }}
          >
            <Typography variant="h4" sx={{ overflowWrap: "anywhere" }}>
              {source.name}
            </Typography>
            <Chip
              label={accountingPeriodName}
              size="small"
              variant="outlined"
            />
          </Stack>
          {canEdit ? (
            <Stack
              direction="row"
              spacing={1}
              sx={{
                width: { xs: "100%", sm: "auto" },
                "& > *": { flex: { xs: 1, sm: "none" } },
              }}
            >
              <Button component={Link} href={editHref} variant="outlined">
                Edit
              </Button>
              <Button
                color="error"
                variant="outlined"
                onClick={() => {
                  setDeleteOpen(true);
                }}
              >
                Delete
              </Button>
            </Stack>
          ) : null}
        </Stack>
      </Stack>
      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns: { xs: "minmax(0, 1fr)", lg: "2fr 1fr" },
          alignItems: "start",
        }}
      >
        <ExpectedIncomeReceivedCard
          source={source}
          income={income}
          returnUrl={currentUrl}
        />
        <Stack spacing={2}>
          <ExpectedIncomePeriodCard source={source} />
          <ExpectedIncomePaymentCard source={source} />
        </Stack>
      </Box>
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

export default ExpectedIncomeSourceDetails;
