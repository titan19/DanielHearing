function hello() { 
    log('Hello World');
};
function sleep(a) {
    if (arguments.length == 0)
        a = 1000;
    delay(a);
};
function say(s)
{
    log(s);
}
function init() {
    register("hello", hello);
    register("sleep", sleep);
    register("say", say);
};