namespace Connextion.Lib

type Posts = Post []
 and Post = Post of string

module Posts =
  let mutable allPosts = [||]
  
  let addPost text =
      let model =  Array.append allPosts [| Post text |]
      allPosts <- model 