"use client";

import { Button, Stack, Typography } from "@mui/material";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import { signOut } from "next-auth/react";

/**
 * Displays the access-denied state for an authenticated identity without application access.
 */
const AccessDeniedView = function (): JSX.Element {
  return (
    <PageLayout>
      <ContentSurface>
        <Typography variant="h5" color="error" sx={{ mb: 2 }}>
          Access Unavailable
        </Typography>
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
      </ContentSurface>
    </PageLayout>
  );
};

export default AccessDeniedView;
