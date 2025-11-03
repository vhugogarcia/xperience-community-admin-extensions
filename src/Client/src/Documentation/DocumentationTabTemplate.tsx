import React, { useState } from "react";
import {
  Stack,
  Headline,
  HeadlineSize,
  Spacing,
} from "@kentico/xperience-admin-components";

enum PageAvailabilityStatus {
  Available,
  NotAvailable,
}

interface DocumentationTabTemplateProps {
  pageAvailability: PageAvailabilityStatus;
  htmlContent?: string | null;
  contentTypeName?: string | null;
  errorMessage?: string | null;
}

export const DocumentationTabTemplate = (
  props: DocumentationTabTemplateProps | null
) => {
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [htmlContent, setHtmlContent] = useState<string | undefined | null>(
        props?.htmlContent
    );
    const [isDocumentationAvailable, setIsDocumentationAvailable] = useState<PageAvailabilityStatus | undefined | null>(
        props?.pageAvailability
    );

  // Validate props and check availability
  if (isDocumentationAvailable === PageAvailabilityStatus.NotAvailable) {
    return (
      <div style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        minHeight: '60vh',
        width: '100%'
      }}>
        <Stack spacing={Spacing.M}>
          <div style={{ textAlign: 'center' }}>
            <Headline size={HeadlineSize.M}>
              Documentation Not Available
            </Headline>
          </div>
        </Stack>
      </div>
    );
  }

  return (
    <div style={{ padding: '1.5rem' }}>
      <Stack spacing={Spacing.XL}>
        <Stack spacing={Spacing.M}>
          <Headline size={HeadlineSize.L}>
              Documentation
          </Headline>
        </Stack>
        <div
          className="documentation-content"
          style={{
            color: '#333',
            fontSize: '14px',
            lineHeight: '1.6'
          }}
          dangerouslySetInnerHTML={{
            __html: htmlContent || "",
          }}
        />
      </Stack>
    </div>
  );
};