import '@/styles/globals.css'
import type { AppProps } from 'next/app'
import DodgeballConfigurationScreen, {ResetConfigButton} from "@/components/Configuration/DodgeballConfigurationScreen";

import { useDodgeball } from "@dodgeball/trust-sdk-client";
import { useMemo, useState, useEffect } from "react";
import { useRouter } from 'next/router'

const dodgeballApiKey = process.env.NEXT_PUBLIC_DODGEBALL_PUBLIC_KEY;
const dodgeballUrl = process.env.NEXT_PUBLIC_DODGEBALL_API_URL || "https://api.dev.dodgeballhq.com/";


function MyApp({ Component, pageProps }) {
  console.log("Public API Key", process.env.NEXT_PUBLIC_DODGEBALL_PUBLIC_KEY)
  const router = useRouter();
  const [pageHasLoaded, setPageHasLoaded] = useState(false);
  const [isDodgeballConfigured, setIsDodgeballConfigured] = useState(false);
  const [dodgeballKeys, setDodgeballKeys] = useState({
    publicKey: null,
    privateKey: null
  });

  const hasConfiguredDodgeballKeys = useMemo(() => {
    return dodgeballKeys.publicKey != null && dodgeballKeys.privateKey != null;
  }, [dodgeballKeys]);


  useEffect(() => {
    if (dodgeballKeys.publicKey != null && dodgeballKeys.privateKey != null) {
      const dodgeball = useDodgeball(dodgeballKeys.publicKey, {
        apiUrl: dodgeballUrl
      });
    }
  }, [dodgeballKeys]);

  useEffect(() => {
    if (typeof window !== 'undefined') {
      setTimeout(() => {
        setPageHasLoaded(true);
      }, 20);

      if (window.localStorage.getItem('DODGEBALL_PUBLIC_KEY') != null && window.localStorage.getItem('DODGEBALL_PRIVATE_KEY') != null) {
        setDodgeballKeys({
          publicKey: window.localStorage.getItem('DODGEBALL_PUBLIC_KEY'),
          privateKey: window.localStorage.getItem('DODGEBALL_PRIVATE_KEY')
        });
        setTimeout(() => {
          setIsDodgeballConfigured(true);
        }, 10);
      }
    }
  }, []);

  const handleSetKeys = (publicKey, privateKey) => {
    setDodgeballKeys({
      publicKey: publicKey,
      privateKey: privateKey
    });
    window.localStorage.setItem('DODGEBALL_PUBLIC_KEY', publicKey);
    window.localStorage.setItem('DODGEBALL_PRIVATE_KEY', privateKey);

    setTimeout(() => {
      setIsDodgeballConfigured(true);
    }, 1000);
  }

  const handleResetConfig = () => {
    setDodgeballKeys({
      publicKey: null,
      privateKey: null
    });
    window.localStorage.removeItem('DODGEBALL_PUBLIC_KEY');
    window.localStorage.removeItem('DODGEBALL_PRIVATE_KEY');
    setIsDodgeballConfigured(false);
    router.reload(window.location.pathname);
  }

  if (!pageHasLoaded) {
    return null;
  }

  if (!hasConfiguredDodgeballKeys || !isDodgeballConfigured) {
    return (
      <DodgeballConfigurationScreen setKeys={handleSetKeys}/>
    );
  }

  return (
    <>
      <Component {...pageProps} />
  <ResetConfigButton onReset={handleResetConfig}/>
      </>
  );
}

export default MyApp;


/*
export default function App({ Component, pageProps }: AppProps) {
  return <Component {...pageProps} />
}

 */
