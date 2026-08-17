"use client";

import { Avatar, Box, Button, Stack, Typography } from "@mui/material";
import { AdminPanelSettings } from "@mui/icons-material";
import ColorSchemeSelector from "@/framework/navigation/ColorSchemeSelector";
import type { CurrentApplicationUser } from "@/framework/auth/currentApplicationUser";
import type { JSX } from "react";
import { UserRoleModel } from "@/framework/data/api";
import { signOut } from "next-auth/react";

/**
 * Props for the CurrentUserMenu component.
 */
interface CurrentUserMenuProps {
  readonly applicationUser: CurrentApplicationUser | null;
  readonly onNavigate?: (() => void) | undefined;
  readonly user:
    | {
        readonly name?: string | null;
        readonly email?: string | null;
        readonly image?: string | null;
      }
    | undefined;
}

/**
 * Displays the signed-in user's identity and account actions.
 */
const CurrentUserMenu = function ({
  applicationUser,
  onNavigate,
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
      <ColorSchemeSelector />
      {applicationUser?.role === UserRoleModel.Admin ? (
        <Button
          fullWidth
          href="/admin/users"
          startIcon={<AdminPanelSettings />}
          sx={{ mt: 2 }}
          variant="outlined"
          {...(onNavigate === undefined ? {} : { onClick: onNavigate })}
        >
          Manage users
        </Button>
      ) : null}
      <Button
        fullWidth
        sx={{ mt: 1 }}
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
