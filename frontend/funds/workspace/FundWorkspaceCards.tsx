"use client";

import { Box, Button, ButtonBase, Stack, Typography } from "@mui/material";
import { useRouter, useSearchParams } from "next/navigation";
import Frame from "@/framework/view/Frame";
import type { FundWithBalance } from "@/funds/types";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/FundWorkspace";
import type { JSX } from "react";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/funds/routes";

/**
 * Props for the FundWorkspaceCards component.
 */
interface FundWorkspaceCardsProps {
  readonly data: FundWithBalance[] | null;
  readonly isInOnboardingMode: boolean;
}

const searchParamName = "search";

/**
 * Displays the fund workspace as a collection of clickable cards.
 */
const FundWorkspaceCards = function ({
  data,
  isInOnboardingMode,
}: FundWorkspaceCardsProps): JSX.Element {
  const router = useRouter();
  const searchParams = useSearchParams();
  const hasSearch = (searchParams.get(searchParamName) ?? "").trim() !== "";
  const funds = data ?? [];
  const updateParams = useSearchParamUpdater([]);

  const clearSearch = function (): void {
    updateParams((params) => {
      params.delete(searchParamName);
    });
  };

  const openFund = function (fundId: string): void {
    const search = searchParams.get(searchParamName);
    const detailSearchParams: FundWorkspaceSearchParams =
      search === null ? {} : { search };
    router.push(routes.workspaceDetail(fundId, detailSearchParams), {
      scroll: false,
    });
  };

  if (funds.length === 0) {
    return (
      <Stack spacing={2} alignItems="flex-start">
        <Typography color="text.secondary">
          {hasSearch
            ? "No funds match the current search. Try a different name."
            : isInOnboardingMode
              ? "Use onboarding to add the first fund to your workspace."
              : "Create a fund to start building your workspace."}
        </Typography>
        {hasSearch ? (
          <Button variant="contained" onClick={clearSearch}>
            Clear search
          </Button>
        ) : null}
      </Stack>
    );
  }

  return (
    <Box
      sx={{
        display: "grid",
        gap: 2,
        justifyContent: "start",
        justifyItems: "stretch",
        alignItems: "start",
        gridTemplateColumns: {
          xs: "minmax(0, 1fr)",
          sm: "repeat(auto-fit, minmax(280px, max-content))",
        },
      }}
    >
      {funds.map((fund) => (
        <ButtonBase
          key={fund.id}
          onClick={() => {
            openFund(fund.id);
          }}
          sx={{
            display: "flex",
            width: "100%",
            minWidth: 0,
            borderRadius: 5,
            textAlign: "left",
            "& .MuiPaper-root": { width: "100%" },
          }}
        >
          <Frame
            title={fund.name}
            color={fund.currentBalance.postedBalance >= 0 ? "info" : "error"}
            headerContent={
              <KeyboardArrowRight
                sx={{ color: "text.secondary", fontSize: 22 }}
              />
            }
          >
            <Stack spacing={0.5}>
              <Typography
                variant="overline"
                sx={{ color: "text.secondary", fontWeight: 700 }}
              >
                Current balance
              </Typography>
              <Typography variant="h5">
                {formatCurrency(fund.currentBalance.postedBalance)}
              </Typography>
            </Stack>
          </Frame>
        </ButtonBase>
      ))}
    </Box>
  );
};

export default FundWorkspaceCards;
