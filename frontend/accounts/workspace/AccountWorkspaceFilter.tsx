"use client";

import { Paper, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";

/**
 * Renders the filter card for the Accounts workspace with header and search bar.
 */
const AccountWorkspaceFilter = function (): JSX.Element {
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
          <Typography variant="h5">Accounts Workspace</Typography>
        </Stack>
        <SearchBar searchParamName="search" pageParamName="page" />
      </Stack>
    </Paper>
  );
};

export default AccountWorkspaceFilter;
