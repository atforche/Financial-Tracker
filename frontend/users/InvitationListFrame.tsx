"use client";

import { type JSX, useState } from "react";
import { formatDate, formatUserRole } from "@/users/userManagementHelpers";
import { getPaginationIndex, getRowsPerPage } from "@/framework/listframe/page";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ListFrame from "@/framework/listframe/ListFrame";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import ManageInvitationDialog from "@/users/ManageInvitationDialog";
import type { UserInvitation } from "@/users/types";
import { useSearchParams } from "next/navigation";

/**
 * Props for the paged invitation-history list.
 */
interface InvitationListFrameProps {
  readonly invitations: readonly UserInvitation[];
}

const pageParamName = "invitationsPage";

/**
 * Displays invitation history with pending-invitation actions.
 */
const InvitationListFrame = function ({
  invitations,
}: InvitationListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const [managedInvitation, setManagedInvitation] =
    useState<UserInvitation | null>(null);
  const rowsPerPage = getRowsPerPage(searchParams.get("pageSize"));
  const paginationIndex = getPaginationIndex(
    searchParams.get(pageParamName),
    invitations.length,
    rowsPerPage,
  );
  const pageInvitations = invitations.slice(
    paginationIndex * rowsPerPage,
    (paginationIndex + 1) * rowsPerPage,
  );
  const columns: readonly ColumnDefinition<UserInvitation>[] = [
    {
      name: "email",
      headerContent: "Email",
      getBodyContent: (invitation) => invitation.email,
    },
    {
      name: "role",
      headerContent: "Role",
      getBodyContent: (invitation) => formatUserRole(invitation.role),
    },
    {
      name: "status",
      headerContent: "Status",
      getBodyContent: (invitation) => invitation.status,
    },
    {
      name: "created",
      headerContent: "Created",
      getBodyContent: (invitation) => formatDate(invitation.createdAt),
    },
    {
      name: "accepted",
      headerContent: "Accepted",
      getBodyContent: (invitation) => formatDate(invitation.acceptedAt),
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (invitation) => (
        <ListFrameActionButton
          size="small"
          color="primary"
          onClick={() => {
            setManagedInvitation(invitation);
          }}
          ariaLabel={`Open invitation for ${invitation.email}`}
        >
          <ArrowForwardOutlined fontSize="small" color="action" />
        </ListFrameActionButton>
      ),
      alignment: "right",
      minWidth: 52,
      maxWidth: 52,
    },
  ];

  return (
    <>
      <ListFrame<UserInvitation>
        title="Invitation History"
        columns={columns}
        getId={(invitation) => invitation.id}
        data={pageInvitations}
        totalCount={invitations.length}
        pageParamName={pageParamName}
        hasActiveFilters={false}
        onRowClick={(invitation) => {
          setManagedInvitation(invitation);
        }}
        initialEmptyState={{
          title: "No Invitations",
          description: "Create an invitation to grant a collaborator access.",
        }}
      />
      {managedInvitation !== null ? (
        <ManageInvitationDialog
          invitation={managedInvitation}
          open
          onClose={() => {
            setManagedInvitation(null);
          }}
        />
      ) : null}
    </>
  );
};

export default InvitationListFrame;
