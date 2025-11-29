import {
  Box,
  Paper,
  Table,
  TableBody,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import type { JSX } from "react";

/**
 * Props for the ListFrame component.
 * @param {string} name - Name to display at the top of the list frame.
 * @param {JSX.Element[]} headers - Array of JSX elements representing the table headers.
 * @param {(item: T) => JSX.Element[]} columns - Function that takes an item of type T and returns an array of JSX elements representing the table columns.
 * @param {(item: T) => string} getId - Function that takes an item of type T and returns its unique identifier as a string.
 * @param {T[]} data - Array of data items of type T to be displayed in the list frame.
 * @param {React.ReactNode} children - Child components to be rendered within the ListFrame.
 */
interface ListFrameProps<T> {
  readonly name: string;
  readonly headers: JSX.Element[];
  readonly columns: (item: T) => JSX.Element[];
  readonly getId: (item: T) => string;
  readonly data: T[];
  readonly children: React.ReactNode;
}

/**
 * Component that presents a generic list frame with a table structure.
 * @template T - Type of the data items to be displayed in the list frame.
 * @param props - Props for the ListFrame component.
 * @returns JSX element representing the ListFrame.
 */
const ListFrame = function <T>({
  name,
  headers,
  columns,
  getId,
  data,
  children,
}: ListFrameProps<T>): JSX.Element {
  return (
    <Box sx={{ paddingLeft: "25px" }}>
      <Typography
        variant="h6"
        sx={{ paddingBottom: "25px", paddingTop: "25px" }}
      >
        {name}
      </Typography>
      <Paper
        sx={{
          [`& .MuiIconButton-root`]: {
            display: "none",
          },
          [`& .MuiTableRow-hover:hover`]: {
            [`.MuiIconButton-root`]: {
              display: "inline-block",
            },
          },
          width: "100%",
        }}
      >
        <TableContainer>
          <Table stickyHeader>
            <TableHead>
              <TableRow>{headers}</TableRow>
            </TableHead>
            <TableBody>
              {data.map((item) => (
                <TableRow hover role="checkbox" tabIndex={-1} key={getId(item)}>
                  {columns(item)}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>
      {children}
    </Box>
  );
};

export default ListFrame;
