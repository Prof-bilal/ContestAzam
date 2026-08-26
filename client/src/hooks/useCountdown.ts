import { useCallback, useEffect, useRef, useState } from "react";

/// A single-interval countdown. Guarantees only one timer runs at a time and
/// cleans up on unmount, so React re-renders never spawn duplicate intervals.
export function useCountdown() {
  const [seconds, setSeconds] = useState(0);
  const intervalRef = useRef<number | undefined>(undefined);

  const clear = useCallback(() => {
    if (intervalRef.current !== undefined) {
      window.clearInterval(intervalRef.current);
      intervalRef.current = undefined;
    }
  }, []);

  const start = useCallback(
    (from: number) => {
      clear();
      setSeconds(from);
      intervalRef.current = window.setInterval(() => {
        setSeconds((prev) => {
          if (prev <= 1) {
            clear();
            return 0;
          }
          return prev - 1;
        });
      }, 1000);
    },
    [clear],
  );

  useEffect(() => clear, [clear]);

  return { seconds, start, active: seconds > 0 };
}
