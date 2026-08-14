"use client";

import { Button, Stack, Typography } from "@mui/material";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";

/**
 * Props for the application error boundary.
 */
interface ErrorPageProps {
  readonly error: Error & { digest?: string };
  readonly reset: () => void;
}

/**
 * Displays a recoverable error state for an application route.
 */
const ErrorPage = function ({ error, reset }: ErrorPageProps): JSX.Element {
  return (
    <PageLayout>
      <Frame title="Unable To Load This Page" color="error">
        <Stack spacing={2} alignItems="flex-start">
          <Typography>
            Something went wrong while loading the requested data. Try the
            request again.
          </Typography>
          <Button
            variant="contained"
            color="error"
            onClick={() => {
              reset();
            }}
          >
            Try again
          </Button>
          {typeof error.digest === "string" ? (
            <Typography variant="caption" color="text.secondary">
              Reference: {error.digest}
            </Typography>
          ) : null}
        </Stack>
      </Frame>
    </PageLayout>
  );
};

export default ErrorPage;
