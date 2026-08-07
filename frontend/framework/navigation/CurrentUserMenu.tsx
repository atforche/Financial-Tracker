"use client";

import { Avatar, Box, Button, Stack, Typography } from "@mui/material";
import type { CurrentApplicationUser } from "@/framework/auth/currentApplicationUser";
import type { JSX } from "react";
import { signOut } from "next-auth/react";

/**
 * Props for the CurrentUserMenu component.
 */
interface CurrentUserMenuProps {
  readonly applicationUser: CurrentApplicationUser | null;
  readonly user:
    | {
        readonly name?: string | null;
        readonly email?: string | null;
        readonly image?: string | null;
      }
    | undefined;
}

/**
 * Displays the signed-in user's identity and a logout action.
 */
const CurrentUserMenu = function ({
  applicationUser,
  user,
}: CurrentUserMenuProps): JSX.Element | null {
  if (user === undefined) {
    return null;
  }

  const name =
    applicationUser?.displayName ?? user.name ?? user.email ?? "Signed in user";
  const email = applicationUser?.email ?? user.email;

  return (
    <Box sx={{ mt: "auto", p: 2 }}>
      <Stack direction="row" spacing={1.5} alignItems="center">
        <Avatar src={user.image ?? undefined} alt="">
          {name.slice(0, 1).toUpperCase()}
        </Avatar>
        <Box sx={{ minWidth: 0 }}>
          <Typography noWrap variant="body2">
            {name}
          </Typography>
          {email !== null && email !== undefined && email !== "" ? (
            <Typography noWrap color="text.secondary" variant="caption">
              {email}
            </Typography>
          ) : null}
        </Box>
      </Stack>
      <Button
        fullWidth
        sx={{ mt: 2 }}
        variant="outlined"
        onClick={() => {
          signOut({ redirectTo: "/login" }).catch(() => undefined);
        }}
      >
        Log out
      </Button>
    </Box>
  );
};

export default CurrentUserMenu;
