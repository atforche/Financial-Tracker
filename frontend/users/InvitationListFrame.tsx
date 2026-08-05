"use client";

import { getPaginationIndex, rowsPerPage } from "@/framework/listframe/page";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import InvitationActions from "@/users/InvitationActions";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import type { UserInvitation } from "@/users/types";
import { formatDate } from "@/users/userManagementHelpers";
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
  const paginationIndex = getPaginationIndex(
    searchParams.get(pageParamName),
    invitations.length,
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
      getBodyContent: (invitation) => invitation.role,
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
      headerContent: "Actions",
      getBodyContent: (invitation) => (
        <InvitationActions invitation={invitation} />
      ),
    },
  ];

  return (
    <ListFrame<UserInvitation>
      title="Invitation history"
      columns={columns}
      getId={(invitation) => invitation.id}
      data={pageInvitations}
      totalCount={invitations.length}
      pageParamName={pageParamName}
      hasActiveFilters={false}
      initialEmptyState={{
        title: "No invitations",
        description: "Create an invitation to grant a collaborator access.",
      }}
    />
  );
};

export default InvitationListFrame;
