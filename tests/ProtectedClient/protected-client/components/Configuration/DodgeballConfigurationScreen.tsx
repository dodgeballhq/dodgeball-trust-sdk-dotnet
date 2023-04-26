import { useMemo, useState } from "react";

const dodgeballUrl =
  process.env.NEXT_PUBLIC_DODGEBALL_API_URL ||
  "https://api.dev.dodgeballhq.com/";

export default function DodgeballConfigurationScreen({ setKeys }) {
  const [publicKey, setPublicKey] = useState("");
  const [privateKey, setPrivateKey] = useState("");

  const isMissingConfig = useMemo(() => {
    return !publicKey || !privateKey;
  }, [publicKey, privateKey]);

  const handleSaveClick = () => {
    setKeys(publicKey, privateKey);
  };

  return (
    <div className="bg-gray-100 fixed left-0 right-0 top-0 bottom-0">
      <div className="ml-auto mr-auto mt-[100px] w-[400px] px-[20px] py-[20px] rounded-md shadow-lg bg-white">
        <h1 className="w-full text-xl pb-[10px]">Dodgeball Configuration</h1>
        <div className="text-xs pb-[10px]">
          This version of Bondfire is configured to send Dodgeball API requests
          to: <span className="font-semibold">{dodgeballUrl}</span>
        </div>
        <div className="text-xs pb-[10px]">
          Enter the private and public API keys for a Dodgeball account in that
          environment to begin testing.
        </div>
        <div className="text-sm pb-[10px]">
          <label htmlFor="publicKey" className="block w-full pb-[5px]">
            Public Key
          </label>
          <input
            className="block w-full rounded-md"
            name="publicKey"
            type="text"
            placeholder="Public Key"
            value={publicKey}
            onChange={(e) => setPublicKey(e.target.value)}
          />
        </div>
        <div className="text-sm">
          <label htmlFor="privateKey" className="block w-full pb-[5px]">
            Private Key
          </label>
          <input
            className="block w-full rounded-md"
            name="privateKey"
            type="text"
            placeholder="Private Key"
            value={privateKey}
            onChange={(e) => setPrivateKey(e.target.value)}
          />
        </div>
        <div className="text-sm flex justify-end pt-[20px]">
          <button
            className="px-[10px] py-[6px] rounded-md text-green-700 bg-green-100 border border-green-700 hover:bg-green-600 hover:text-white disabled:bg-gray-100 disabled:border-gray-400 disabled:text-gray-400"
            disabled={isMissingConfig}
            onClick={handleSaveClick}
          >
            Save
          </button>
        </div>
      </div>
    </div>
  );
}

export function ResetConfigButton({ onReset }) {
  const handleResetClick = () => {
    onReset();
  };

  return (
    <div className="fixed z-[99999] left-[20px] bottom-[20px]">
      <button
        className="px-[10px] py-[6px] rounded-md text-red-700 bg-red-100 border border-red-700 hover:bg-red-600 hover:text-white"
        onClick={handleResetClick}
      >
        Reset
      </button>
    </div>
  );
}
