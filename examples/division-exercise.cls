(def r (new Random))
(println "Division Exercise\n")
(loop [score 0]
  (let [b (+ 1 (. r Next 10))
        c (. r Next 100)
        a (* b c)
        _ (println "Score: " score "\n")
        _ (print a "/" b "= ")
        line (read-line)]
    (when line
      (if (= (.ToString c) line)
        (do (println "Correct!")
            (recur (+ 1 score)))
        (do (println "Wrong! Correct answer is" c)
            (recur score))))))
