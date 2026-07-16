"use client";

import { Button, Paper, Stack, Typography } from "@mui/material";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/FundWorkspace";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import nameof from "@/framework/data/nameof";
import routes from "@/funds/routes";

/**
 * Props for the FundWorkspaceFilter component.
 */
interface FundWorkspaceFilterProps {
  readonly isInOnboardingMode: boolean;
}

/**
 * Renders the filter card for the Fund workspace with header, search bar, and primary action.
 */
const FundWorkspaceFilter = function ({
  isInOnboardingMode,
}: FundWorkspaceFilterProps): JSX.Element {
  const addFundHref = isInOnboardingMode
    ? routes.workspaceOnboard({})
    : routes.workspaceCreate({});

  return (
    <Paper
      sx={{
        top: 10,
        zIndex: (theme) => theme.zIndex.appBar - 1,
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        bgcolor: "background.paper",
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2}>
        <Stack spacing={0.5}>
          <Typography variant="h5">Fund Workspace</Typography>
        </Stack>
        <Stack
          direction="row"
          spacing={1.5}
          useFlexGap
          flexWrap={{ xs: "wrap", md: "nowrap" }}
          alignItems={{ xs: "stretch", md: "center" }}
        >
          <SearchBar
            searchParamName={nameof<FundWorkspaceSearchParams>("search")}
          />
          <Button variant="contained" href={addFundHref} sx={{ flexShrink: 0 }}>
            {isInOnboardingMode ? "Onboard fund" : "Create fund"}
          </Button>
        </Stack>
      </Stack>
    </Paper>
  );
};

export default FundWorkspaceFilter;
