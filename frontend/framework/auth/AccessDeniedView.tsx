"use client";

import { Button, Stack, Typography } from "@mui/material";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import { signOut } from "next-auth/react";

/**
 * Displays the access-denied state for an authenticated identity without application access.
 */
const AccessDeniedView = function (): JSX.Element {
  return (
    <PageLayout>
      <Frame title="Access Unavailable" color="error">
        <Stack spacing={2} alignItems="flex-start">
          <Typography>
            This account is not currently authorized to use Financial Tracker.
            Contact an administrator if you believe this is an error.
          </Typography>
          <Button
            color="error"
            variant="contained"
            onClick={() => {
              signOut({ redirectTo: "/login" }).catch(() => undefined);
            }}
          >
            Return to sign in
          </Button>
        </Stack>
      </Frame>
    </PageLayout>
  );
};

export default AccessDeniedView;
