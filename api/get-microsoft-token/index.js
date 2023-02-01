const { chromium } = require("playwright-chromium");
const puppeteer = chromium

module.exports = async function (context, req) {
    const userName = (req.query.userName || (req.body && req.body.userName))
    const password = (req.query.password || (req.body && req.body.password))

    if(!userName || !password) {
        context.res = {
            status: 404,
            body: 'Email and Password are required!'
        }
        return
    }
    if(!userName.match('[@\\\\]') || password.length > 128) {
        context.res = {
            status: 422,
            body: 'Wrong Email or Password!'
        }
        return
    }

    const browser = await puppeteer.launch({ headless: true, defaultViewport: null, args: ["--disable-notifications"] })
    const page = await browser.newPage()
    
    await page.goto('https://ctsv.hust.edu.vn/#/login', { waitUntil: 'domcontentloaded' })
    await page.click('button.el-button.el-button--danger.is-plain')
    await page.waitForSelector("span#nextButton", { visible: true })
    
    await page.$eval("input[name='UserName']", (el, val) => el.value = val, userName)
    await page.click('span#nextButton')

    await page.$eval("input[name='Password']", (el, val) => el.value = val, password)
    await page.click("span#submitButton")

    if(await page.$eval('#errorText', () => true).catch(() => false)) { // wrong password
        context.res = {
            status: 422,
            body: 'Wrong Email or Password!'
        }
        browser.close()
        return
    }
    // await page.waitForSelector("img#img-banner", { visible: true })
    
    const token = await page.evaluate(() => {
        for (let i = 0; i < localStorage.length; i++) {
            const key = localStorage.key(i)
            if(key == 'adal.idtoken') return localStorage.getItem(key)
        }
        return ''
    })
    
      
    browser.close()

    context.res = {
        body: token
    }
}