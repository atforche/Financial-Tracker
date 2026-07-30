/* eslint-disable sort-imports */
import { Box, Button, Stack, Typography } from "@mui/material";
import Image, { type StaticImageData } from "next/image";
import { auth, signIn } from "@/auth";
import { redirect } from "next/navigation";
import type { JSX } from "react";
import Frame from "@/framework/view/Frame";
import applicationIcon from "@/app/icon.svg";

/**
 * Confirms that a statically imported image has the shape expected by Next.js.
 */
const isStaticImageData = function (value: unknown): value is StaticImageData {
  return (
    typeof value === "object" &&
    value !== null &&
    "src" in value &&
    typeof value.src === "string" &&
    "width" in value &&
    typeof value.width === "number" &&
    "height" in value &&
    typeof value.height === "number"
  );
};

if (!isStaticImageData(applicationIcon)) {
  throw new Error("The Financial Tracker icon could not be loaded.");
}

const loginIcon = applicationIcon;

/**
 * Displays the sign-in page for the application.
 */
const LoginPage = async function ({
  searchParams,
}: {
  readonly searchParams: Promise<{ callbackUrl?: string }>;
}): Promise<JSX.Element> {
  const { callbackUrl } = await searchParams;
  const redirectTo =
    typeof callbackUrl === "string" && callbackUrl.startsWith("/")
      ? callbackUrl
      : "/";
  const session = await auth();

  if (session !== null) {
    redirect(redirectTo);
  }

  return (
    <Box
      sx={{
        alignItems: "center",
        backgroundColor: "background.default",
        backgroundImage:
          "linear-gradient(135deg, rgba(25, 118, 210, 0.1), transparent 58%)",
        display: "flex",
        justifyContent: "center",
        minHeight: "100vh",
        p: { xs: 2, sm: 3 },
      }}
    >
      <Box sx={{ maxWidth: 460, width: "100%" }}>
        <Frame title="Welcome back">
          <Stack
            alignItems="center"
            spacing={2.5}
            sx={{ px: { xs: 1, sm: 2 }, py: 2 }}
          >
            <Image
              src={loginIcon}
              alt="Financial Tracker"
              height={84}
              width={99}
              priority
            />
            <Stack alignItems="center" spacing={0.75} textAlign="center">
              <Typography component="h1" variant="h4">
                Financial Tracker
              </Typography>
              <Typography color="text.secondary">
                Sign in with your approved Google account to continue.
              </Typography>
            </Stack>
            <form
              style={{ width: "100%" }}
              action={async () => {
                "use server";
                await signIn("google", { redirectTo });
              }}
            >
              <Button fullWidth type="submit" variant="contained">
                Continue with Google
              </Button>
            </form>
          </Stack>
        </Frame>
      </Box>
    </Box>
  );
};

export const dynamic = "force-dynamic";
export default LoginPage;
