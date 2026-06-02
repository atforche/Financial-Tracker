import { Button, Paper, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import routes from "@/accounts/routes";

interface AccountDashboardQuickActionsProps {
  readonly isInOnboardingMode: boolean;
}

/**
 * Displays quick actions for the account dashboard.
 */
const AccountDashboardQuickActions = function ({
  isInOnboardingMode,
}: AccountDashboardQuickActionsProps): JSX.Element {
  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        bgcolor: "background.paper",
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2}>
        <Stack direction="row" spacing={5} alignItems="center" useFlexGap>
          <Typography variant="overline" color="text.secondary">
            Quick actions:
          </Typography>
          {isInOnboardingMode ? (
            <Button
              variant="contained"
              href={routes.onboard}
              sx={{ flexShrink: 0 }}
            >
              Onboard account
            </Button>
          ) : null}
          {!isInOnboardingMode && (
            <Button
              variant="contained"
              href={routes.create()}
              sx={{ flexShrink: 0 }}
            >
              Create account
            </Button>
          )}
        </Stack>
      </Stack>
    </Paper>
  );
};

export default AccountDashboardQuickActions;
