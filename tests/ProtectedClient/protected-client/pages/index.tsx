import Head from "next/head";
import { useState } from "react";
import Router from "next/router";
import { useDodgeball } from "@dodgeball/trust-sdk-client";

export interface ICheckpointForm{
  status: string;
  outcome: string;
  id: string;
}

export default function PostCheckpointForm({
                                       checkpointForm,
                                     }: {
  checkpointForm: ICheckpointForm
}) {
  const dodgeball = useDodgeball();
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [hasPaymentError, setHasPaymentError] = useState<boolean>(false);
  const [paymentErrorMessage, setPaymentErrorMessage] = useState<string>(
    "We were unable to complete your payment at this time. Please contact support for more information."
  );
  const [verificationId, setVerificationId] = useState<string>(
    ""
  );
  const [outcome, setOutcome] = useState<string>(
    ""
  );
  const [status, setStatus] = useState<string>(
    ""
  );

  const submitCheckpoint = async (
    previousVerificationId = null
  ) => {
    setIsLoading(true);
    const sourceToken = await dodgeball.getSourceToken();

    setVerificationId("");
    setOutcome("");
    setStatus("");
    const response = await fetch(
      '/api/ClientTransaction/Transaction',
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        }
      });

    let verificationResponse = await response.json()
    console.log("Verification", verificationResponse)
    setVerificationId(verificationResponse?.verification?.id);
    setOutcome(verificationResponse?.verification?.outcome);
    setStatus(verificationResponse?.verification?.status);
    console.log("verificationId", verificationId)
    console.log("Verification Response", verificationResponse)

    const verification = verificationResponse.verification;
    if (verification) {
      dodgeball.handleVerification(verification, {
        onVerified: async (verification) => {
          setIsLoading(false);

          setVerificationId(verification?.id);
          setOutcome(verification?.outcome);
          setStatus(verification?.status);

          /*
           * This means that a Front End interaction was required to
           * successfully close out the verifications.  For example,
           * in response to an MFA check.  Pass control back to the
           * Server to resume the finalized transformation leveraging
           * the Verification Token
           */
          console.log("VERIFIED", verification);
          verificationResponse = await fetch(
            '/api/ClientTransaction/Transaction',
            {
              method: "POST",
              headers: {
                "Content-Type": "application/json",
                /*
                 * This additional header value indicates it is a
                 * continuation.  The Server should validate against
                 * the underlying transaction ID.
                 */
                "priorVerificationId": verification.id
              }
            });

          console.log("Final Verification Response", verificationResponse)
        },
        onApproved: async (verification) => {
          setIsLoading(false);

          setVerificationId(verification?.id);
          setOutcome(verification?.outcome);
          setStatus(verification?.status);

          /*
           * Display success information to the user.
           */
          console.log("APPROVED", verification);
        },
        onDenied: async (verification) => {
          setIsLoading(false)

          setVerificationId(verification?.id);
          setOutcome(verification?.outcome);
          setStatus(verification?.status);

          /*
           * Display appropriate user message when the request was
           * denied
           */
          console.log("DENIED", verification);
        },
        onError: async (error) => {
          setIsLoading(false)

          setVerificationId(verification?.id);
          setOutcome(verification?.outcome);
          setStatus(verification?.status);

          /*
           * Errors should not occur.  The appropriate business
           * response is usually to proceed but cross-reference causes
           */
          console.log("ERROR", error);
        },
      });
    }
  };

  const handleSubmit = async (
    e: ChangeEvent<HTMLInputElement> | FormEvent<HTMLButtonElement>
  ) => {
    e.preventDefault();
    await submitCheckpoint()

    if (isLoading) {
      return;
    }
  }

  return (
    <form className="relative z-20 grid grid-cols-1 gap-x-16 max-w-7xl mx-auto lg:px-8 lg:grid-cols-2 xl:gap-x-48">
      <section
        className="bg-gray-50 pt-16 pb-10 px-4 sm:px-6 lg:px-0 lg:pb-16 lg:bg-transparent lg:row-start-1 lg:col-start-2">
        <div className="max-w-lg mx-auto lg:max-w-none">
          <section className="mt-10">
            <h2 className="text-lg font-medium text-gray-900">Invoke a Checkpoint</h2>

            <div className="mt-6 grid grid-cols-3 sm:grid-cols-4 gap-y-6 gap-x-4">
              <div className="col-span-3 sm:col-span-4">
                <button
                  type="submit"
                  disabled={isLoading}
                  onClick={(e) => {
                    handleSubmit(e);
                  }}
                  onSubmit={(e) => {
                    handleSubmit(e);
                  }}
                  className="mt-6 p-3 w-full border border-transparent shadow-xl text-lg font-medium rounded-md text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50"
                >
                  {isLoading ? "Processing..." : "Submit Payment"}
                </button>
              </div>
            </div>
          </section>
          <div>
            <section>
              <h2 className="text-lg font-medium text-gray-900">Checkpoint Response</h2>

              <div className="mt-6 grid grid-cols-3 sm:grid-cols-4 gap-y-6 gap-x-4">
                <div className="col-span-3 sm:col-span-4">
                  Verification ID: {verificationId}
                </div>

                <div className="col-span-3 sm:col-span-4">
                  Outcome: {outcome}
                </div>
                <div className="col-span-3 sm:col-span-4">
                  Status: {status}
                </div>
              </div>
            </section>
          </div>
        </div>
      </section>
    </form>);
}